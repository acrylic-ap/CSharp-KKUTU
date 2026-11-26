using Model;
using Server.Models;
using System.Collections.Concurrent;
using System.Net;

namespace Server.Manager
{
    internal static class MatchingManager
    {
        private static readonly ConcurrentQueue<Player> waitingPlayers = new();
        private static readonly ConcurrentQueue<Player> readyPlayers = new();

        private static readonly ConcurrentDictionary<int, GameSession> sessions = new();
        private static readonly ConcurrentDictionary<int, GameRoom> rooms = new();
        private static int sessionId = 0;

        public static void Enqueue(Player player)
        {
            waitingPlayers.Enqueue(player);

            Console.WriteLine($"매칭 대기열 등록: {player.Nickname}");
        }

        private static void CreateSession(
    ConcurrentDictionary<IPAddress, ClientConnection> connections)
        {
            while (readyPlayers.Count >= 2)
            {
                readyPlayers.TryDequeue(out Player? first);
                readyPlayers.TryDequeue(out Player? second);

                if (!connections.TryGetValue(first!.IP, out ClientConnection? firstConnection)
                    || firstConnection.Player != first)
                {
                    readyPlayers.Enqueue(second!);
                    continue;
                }

                if (!connections.TryGetValue(second!.IP, out ClientConnection? secondConnection)
                    || secondConnection.Player != second)
                {
                    readyPlayers.Enqueue(first);
                    continue;
                }

                GameSession session = new(
                    ++sessionId,
                    firstConnection,
                    secondConnection
                );

                sessions.TryAdd(session.Id, session);

                _ = VerifySession(session);
            }
        }

        private static async Task VerifySession(GameSession session)
        {
            try
            {
                if (!await session.Verify())
                {
                    Console.WriteLine($"접속 검증 실패: {session.Id}");
                    return;
                }

                GameRoom room = new(session);

                if (!rooms.TryAdd(room.Id, room))
                    return;

                try
                {
                    await Task.WhenAll(
                        room.Connections.Select(c =>
                            c.Send($"[MatchSuccess]\n{room.Id}"))
                    );

                    Console.WriteLine($"게임방 생성 완료: {room.Id}");
                }
                catch
                {
                    rooms.TryRemove(room.Id, out _);

                    foreach (ClientConnection connection in room.Connections)
                        connection.Client.Close();

                    throw;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"게임방 생성 오류: {e.Message}");
            }
            finally
            {
                sessions.TryRemove(session.Id, out _);
            }
        }

        public static void ManageQueue()
        {
            if (waitingPlayers.Count + readyPlayers.Count < 2)
                return;

            while (waitingPlayers.TryDequeue(out Player? player))
            {
                if (!player.IsValid)
                    continue;

                readyPlayers.Enqueue(player);

                Console.WriteLine($"매칭 준비 완료: {player.Nickname}");
            }
        }

        public static void Ready(int roomId, ClientConnection connection)
        {
            if (!rooms.TryGetValue(roomId, out GameRoom? room))
                return;

            if (!room.Ready(connection))
                return;

            Console.WriteLine($"모든 플레이어 준비 완료: {roomId}");

            _ = StartGame(room);
        }

        private static async Task StartGame(GameRoom room)
        {
            try
            {
                if (!await HeartbeatManager.Verify(room.Connections))
                    throw new InvalidOperationException("접속 검증 실패");

                room.GameManager.Initialize();
                room.IsStarted = true;

                await Task.WhenAll(
                    room.Connections.Select((c, i) =>
                        c.Send($"[GameStart]\n{room.GameManager.StartChar}\n{i}\n{room.GameManager.ShieldTurn}"))
                );

                Console.WriteLine($"게임 시작: {room.Id}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"게임 시작 실패: {e.Message}");

                rooms.TryRemove(room.Id, out _);

                foreach (ClientConnection connection in room.Connections)
                    connection.Client.Close();
            }
        }

        public static async Task PlayerTurn(int roomId, ClientConnection connection, string word)
        {
            if (!rooms.TryGetValue(roomId, out GameRoom? room))
                return;

            await room.TurnLock.WaitAsync();

            try
            {
                if (!room.IsStarted || room.IsFinished)
                    return;

                if (room.Connections[room.GameManager.CurrentTurn] != connection)
                    return;

                string? err_message = await room.GameManager.CheckWord(word);

                if (err_message is not null)
                {
                    await connection.Send($"[WordError]\n{err_message}");
                    return;
                }

                await CompleteTurn(room, word);
            }
            finally
            {
                room.TurnLock.Release();
            }
        }

        private static async Task<bool> CompleteTurn(GameRoom room, string word)
        {
            room.GameManager.ChangeTurn();

            room.IsFinished = !await room.GameManager.IsExistInitial();

            await Task.WhenAll(
                room.Connections.Select(c => c.Send(
                    $"[WordAccepted]\n{word}\n" +
                    $"{room.GameManager.StartChar}\n" +
                    $"{room.GameManager.CurrentTurn}\n" +
                    $"{room.IsFinished}\n" +
                    $"{room.GameManager.ShieldTurn}"
                ))
            );

            if (room.IsFinished)
            {
                await Task.WhenAll(
                    room.Connections.Select(c => c.Send(
                        $"[GameOver]\n{room.GameManager.CurrentTurn}"
                    ))
                );

                return true;
            }

            return false;
        }

        public static async Task Run(ConcurrentDictionary<IPAddress, ClientConnection> connections)
        {
            using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(100));

            while (await timer.WaitForNextTickAsync())
            {
                ManageQueue();
                CreateSession(connections);
            }
        }
    }
}