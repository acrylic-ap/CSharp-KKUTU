using Utils;

namespace Manager.Manager.Database;

// 한 번 만든 사전은 여러 대국에서 읽기만 한다. 두음으로 빌린 간선도 원래 ID를 사용한다.
internal sealed class StrategicGraph
{
    internal readonly record struct Edge(int First, int Last, int Amount);
    internal readonly char[] Letters;
    internal readonly Edge[] Edges;
    internal readonly int[][] Outgoing;
    internal readonly int[] Doum, Totals;
    internal readonly int[][] Hint;
    internal readonly Dictionary<(char, char), int> EdgeIds = new();
    private readonly Dictionary<char, int> ids;

    internal StrategicGraph(Dictionary<char, Dictionary<char, int>> source, Dictionary<char, int> totals, HashSet<char> kills, HashSet<char> losses)
    {
        Letters = source.Keys.Concat(source.Values.SelectMany(e => e.Keys)).Concat(totals.Keys).Distinct().Order().ToArray();
        ids = Letters.Select((c, i) => (c, i)).ToDictionary(x => x.c, x => x.i);
        Totals = Letters.Select(c => totals.GetValueOrDefault(c)).ToArray();
        Doum = Letters.Select(c => ids.GetValueOrDefault(HangulUtils.GetDoumChar(c), -1)).ToArray();
        var rows = Letters.Select(_ => new List<int>()).ToArray();
        var all = new List<Edge>();
        foreach (char first in Letters)
        {
            if (!source.TryGetValue(first, out var ends)) continue;
            foreach (var (last, amount) in ends.OrderBy(e => e.Key))
            {
                if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(source));
                EdgeIds.Add((first, last), all.Count);
                rows[ids[first]].Add(all.Count);
                all.Add(new(ids[first], ids[last], amount));
            }
        }
        Edges = all.ToArray();
        Outgoing = rows.Select((row, i) => Doum[i] >= 0 && Doum[i] != i ? row.Concat(rows[Doum[i]]).ToArray() : row.ToArray()).ToArray();
        Hint = new int[13][];
        for (int k = 0; k < Hint.Length; k++)
        {
            var layer = Hint[k] = new int[Letters.Length];
            for (int i = 0; i < Letters.Length; i++)
            {
                int best = -190, choices = 0;
                foreach (int id in Outgoing[i])
                {
                    Edge edge = Edges[id];
                    int replies = Capacity(edge, Totals);
                    if (replies < k) continue;
                    choices++;
                    int score = k > 0 ? -Hint[k - 1][edge.Last] : replies == 0 ? 180 : kills.Contains(Letters[edge.Last]) ? 45 : losses.Contains(Letters[edge.Last]) ? -45 : 0;
                    best = Math.Max(best, score);
                }
                // 보호막 해제 시 누가 움직이는지 한 층마다 부호를 바꾼다.
                // 이후 수에서 소모될 간선은 생략한 힌트이며 증명 점수로 쓰지 않는다.
                layer[i] = choices == 0 ? -190 : best * 7 / 8 + Math.Min(12, System.Numerics.BitOperations.Log2((uint)choices) * 2);
            }
        }
    }

    internal int Resolve(char c) => ids.TryGetValue(c, out int i) ? i : ids.GetValueOrDefault(HangulUtils.GetDoumChar(c), -1);
    internal int Capacity(Edge edge, int[] totals)
    {
        int other = Doum[edge.Last];
        return totals[edge.Last] + (other >= 0 && other != edge.Last ? totals[other] : 0) - (edge.First == edge.Last || edge.First == other ? 1 : 0);
    }
}
