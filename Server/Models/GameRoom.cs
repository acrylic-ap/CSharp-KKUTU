using Server.Manager;

namespace Server.Models
{
    internal class GameRoom
    {
        public int Id { get; }

        public ClientConnection[] Connections { get; }

        public MultiPlayManager GameManager { get; }

        private readonly HashSet<ClientConnection> readyPlayers = new();
        private readonly object readyLock = new();

        public SemaphoreSlim TurnLock { get; } = new(1, 1);

        public bool IsStarted { get; set; }
        public bool IsFinished { get; set; }

        public GameRoom(GameSession session)
        {
            Id = session.Id;
            Connections = session.Connections;
            GameManager = new(Connections);
        }

        public bool Ready(ClientConnection connection)
        {
            lock (readyLock)
            {
                if (!Connections.Contains(connection))
                    return false;

                if (!readyPlayers.Add(connection))
                    return false;

                return readyPlayers.Count == Connections.Length;
            }
        }
    }
}