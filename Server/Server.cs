using Manager;
using Model;
using Network;
using Server.Manager;
using Server.Models;
using Server.Utils;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    internal class Server
    {
        private static readonly ConcurrentDictionary<IPAddress, ClientConnection> connections = new();

        private static async Task SendPing(ClientConnection connection)
        {
            try
            {
                long? id = connection.PreparePing();

                if (id is null)
                {
                    connection.Client.Close();
                    return;
                }

                await connection.Send($"[Ping]\n{id}");

                Console.WriteLine($"Ping 전송: {id}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Heartbeat 오류: {e.Message}");

                connection.Client.Close();
            }
        }

        private static async Task Heartbeat()
        {
            using PeriodicTimer timer = new(TimeSpan.FromSeconds(1));

            long lastTick = Environment.TickCount64;

            while (await timer.WaitForNextTickAsync())
            {
                long currentTick = Environment.TickCount64;
                long elapsed = currentTick - lastTick;

                lastTick = currentTick;

                if (elapsed < 800 || elapsed > 1200)
                    continue;

                await Task.WhenAll(
                    connections.Values.Select(SendPing)
                );
            }
        }

        private static async Task HandleClient(TcpClient client)
        {
            using (client)
            {
                IPEndPoint endpoint = (IPEndPoint)client.Client.RemoteEndPoint!;
                IPAddress ip = endpoint.Address;

                ClientConnection connection = new(client);

                if (!connections.TryAdd(ip, connection))
                    return;

                try
                {
                    NetworkStream stream = client.GetStream();

                    string? message;

                    while ((message = await Packet.Receive(stream)) is not null)
                    {
                        string[] parts = message.Split('\n', 2);

                        switch (parts[0])
                        {
                            case "[Setup]":
                                if (connection.Player is not null)
                                    break;

                                if (parts.Length != 2)
                                    break;

                                string nickname = parts[1].Trim();

                                if (nickname == "")
                                    nickname = "익명";

                                connection.Player = new Player(ip, nickname);

                                MatchingManager.Enqueue(connection.Player);

                                Console.WriteLine($"Player가 생성되었습니다. [ip: {ip}, nickname: {nickname}]");

                                break;

                            case "[Pong]":
                                if (parts.Length != 2)
                                    break;

                                if (!long.TryParse(parts[1], out long id))
                                    break;

                                connection.ReceivePong(id);
                                Console.WriteLine($"Pong 수신: {id}");

                                break;

                            case "[Ready]":
                                if (parts.Length != 2)
                                    break;

                                if (!int.TryParse(parts[1], out int roomId))
                                    break;

                                MatchingManager.Ready(roomId, connection);

                                break;

                            case "[Word]":
                                if (parts.Length != 2)
                                    break;

                                string[] wordData = parts[1].Split('\n', 2);

                                if (wordData.Length != 2)
                                    break;

                                if (!int.TryParse(wordData[0], out int wordRoomId))
                                    break;

                                await MatchingManager.PlayerTurn(
                                    wordRoomId, connection, wordData[1]
                                );

                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"통신 오류: {e.Message}");
                }
                finally
                {
                    connection.Player?.Invalidate();

                    connections.TryRemove(ip, out _);

                    Console.WriteLine($"클라이언트 연결 종료: {ip}");
                }
            }
        }

        static async Task Main(string[] args)
        {
            IPAddress? IP = NetworkUtils.GetHamachiIP();

            if (IP is null)
            {
                Console.WriteLine("하마치를 찾지 못했습니다. 연결이 원활한지 확인 후 재실행해 주세요.");
                return;
            }

            try
            {
                await DatabaseManager.StoreStartChars();
                Console.WriteLine("시작 음절 목록 초기화 완료");
            }
            catch (Exception e)
            {
                Console.WriteLine($"DB 초기화 실패: {e}");
                return;
            }

            TcpListener listener = new(IP, 7777);

            listener.Start();

            Console.WriteLine($"서버가 시작되었습니다. ({IP}:7777)");

            _ = Heartbeat();
            _ = MatchingManager.Run(connections);

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();

                Console.WriteLine($"클라이언트 접속: {client.Client.RemoteEndPoint}");

                _ = HandleClient(client);
            }
        }
    }
}