using Model;
using Server.Manager;
using System.Collections.Concurrent;
using System.Net;

namespace Server.Models
{
    internal class GameSession
    {
        public int Id { get; }

        public ClientConnection[] Connections { get; }

        public GameSession(
            int id,
            ClientConnection first,
            ClientConnection second)
        {
            Id = id;
            Connections = [first, second];
        }

        public async Task<bool> Verify()
        {
            return await HeartbeatManager.Verify(Connections);
        }
    }
}