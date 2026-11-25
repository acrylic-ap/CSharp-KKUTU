using Server.Models;

namespace Server.Manager
{
    internal static class HeartbeatManager
    {
        public static async Task<bool> Verify(ClientConnection[] connections)
        {
            long[] initialCounts = connections
                .Select(c => c.PongCount)
                .ToArray();

            long startTime = Environment.TickCount64;

            using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(100));

            while (await timer.WaitForNextTickAsync())
            {
                if (connections.Any(c => c.Player?.IsValid != true))
                    return false;

                if (Environment.TickCount64 - startTime < 3000)
                    continue;

                if (connections.Select((c, i) =>
                    c.PongCount - initialCounts[i] >= 3).All(v => v))
                    return true;

                if (Environment.TickCount64 - startTime >= 10000)
                    return false;
            }

            return false;
        }
    }
}