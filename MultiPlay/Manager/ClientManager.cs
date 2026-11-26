using System.Net.Sockets;
using System.Text;
using Network;

namespace MultiPlay.Manager
{
    public class ClientManager
    {
        private TcpClient? client;
        private string nickname = "유저";
        public event Action<int>? MatchSuccess;

        private NetworkStream? stream;
        private readonly SemaphoreSlim sendLock = new(1, 1);

        public event Action<char, int, int>? GameStart;
        public event Action<string, char, int, bool, int>? WordAccepted;

        public event Action<string>? WordError;
        public event Action<int>? GameOver;

        private async Task Receive(NetworkStream stream, CancellationToken token)
        {
            string? message;

            while ((message = await Packet.Receive(stream, token)) is not null)
            {
                string[] parts = message.Split('\n', 2);

                switch (parts[0])
                {
                    case "[Ping]":
                        if (parts.Length != 2)
                            break;

                        await Packet.Send(stream, $"[Pong]\n{parts[1]}");

                        break;

                    case "[MatchSuccess]":
                        if (parts.Length != 2)
                            break;

                        if (!int.TryParse(parts[1], out int sessionId))
                            break;

                        MatchSuccess?.Invoke(sessionId);

                        break;

                    case "[GameStart]":
                        if (parts.Length != 2)
                            break;

                        string[] gameData = parts[1].Split('\n');

                        if (gameData.Length != 2)
                            break;

                        if (!char.TryParse(gameData[0], out char startChar))
                            break;

                        if (!int.TryParse(gameData[1], out int playerIndex))
                            break;

                        if (!int.TryParse(gameData[2], out int initialShieldTurn))
                            break;

                        GameStart?.Invoke(startChar, playerIndex, initialShieldTurn);

                        break;

                    case "[WordError]":
                        if (parts.Length != 2)
                            break;

                        WordError?.Invoke(parts[1]);

                        break;

                    case "[WordAccepted]":
                        if (parts.Length != 2)
                            break;

                        string[] acceptedData = parts[1].Split('\n');

                        if (acceptedData.Length != 4)
                            break;

                        if (!char.TryParse(acceptedData[1], out char nextChar))
                            break;

                        if (!int.TryParse(acceptedData[2], out int nextTurn))
                            break;

                        if (!bool.TryParse(acceptedData[3], out bool isFinished))
                            break;

                        if (!int.TryParse(acceptedData[4], out int shieldTurn))
                            break;

                        WordAccepted?.Invoke(
                            acceptedData[0], nextChar, nextTurn, isFinished, shieldTurn
                        );

                        break;

                    case "[GameOver]":
                        if (parts.Length != 2)
                            break;

                        if (!int.TryParse(parts[1], out int loserIndex))
                            break;

                        GameOver?.Invoke(loserIndex);

                        break;
                }
            }
        }

        public async Task Send(string message)
        {
            await sendLock.WaitAsync();

            try
            {
                if (stream is null)
                    throw new InvalidOperationException("서버에 연결되어 있지 않습니다.");

                await Packet.Send(stream, message);
            }
            finally
            {
                sendLock.Release();
            }
        }

        public async Task Matching(CancellationToken token)
        {
            using TcpClient client = new();

            using CancellationTokenSource cts =
                CancellationTokenSource.CreateLinkedTokenSource(token);

            cts.CancelAfter(3000);

            await client.ConnectAsync("25.31.207.102", 7777, cts.Token);

            stream = client.GetStream();

            await Packet.Send(stream, $"[Setup]\n{nickname}", token);
            await Receive(stream, token);
        }
    }
}
