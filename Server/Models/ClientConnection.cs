using Network;
using System.Net.Sockets;
using System.Security.Cryptography;
using Model;

namespace Server.Models
{
    internal class ClientConnection
    {
        public TcpClient Client { get; }

        public Player? Player { get; set; }

        public int MissedPongs { get; private set; } = 0;

        private long pingId = 0;
        private bool waitingPong = false;
        private readonly object heartbeatLock = new();

        private long pongCount = 0;
        public long PongCount => Interlocked.Read(ref pongCount);

        private readonly SemaphoreSlim sendLock = new(1, 1);

        public ClientConnection(TcpClient client)
        {
            Client = client;
        }

        public async Task Send(string message)
        {
            using CancellationTokenSource cts = new(1000);

            await sendLock.WaitAsync(cts.Token);

            try
            {
                await Packet.Send(Client.GetStream(), message, cts.Token);
            }
            finally
            {
                sendLock.Release();
            }
        }

        public void ReceivePong(long id)
        {
            lock (heartbeatLock)
            {
                if (!waitingPong || id != pingId)
                    return;

                waitingPong = false;
                MissedPongs = 0;

                Interlocked.Increment(ref pongCount);
            }
        }

        public long? PreparePing()
        {
            lock (heartbeatLock)
            {
                if (waitingPong)
                    MissedPongs++;

                if (MissedPongs >= 3)
                    return null;

                waitingPong = true;

                pingId = BitConverter.ToInt64(
                    RandomNumberGenerator.GetBytes(8)
                );

                return pingId;
            }
        }

        public void MissPong()
        {
            MissedPongs++;
        }

        public bool IsAlive()
        {
            return MissedPongs < 5;
        }
    }
}