using System.Diagnostics;
using System.Numerics;

namespace Manager.Manager.Database;

internal sealed class StrategicSearch
{
    internal readonly record struct Move(char First, char Last);
    internal readonly record struct Observation(int Depth, int Order, Move Edge, int Score, string Bound);
    private const int Win = 1_000_000, Limit = 2 * Win, MaxDepth = 64, Forced = 32;
    private readonly StrategicGraph graph;
    private readonly int[] left, totals;
    private readonly List<int>[] frames = Enumerable.Range(0, MaxDepth + Forced + 2).Select(_ => new List<int>()).ToArray();
    private readonly bool measure;
    internal List<Observation>? Trace { get; }
    internal int? BestScore { get; private set; }
    internal int CompletedDepth { get; private set; }
    internal int SelectedDepth { get; private set; }
    internal long Visits { get; private set; }
    internal long EvalTicks { get; private set; }
    internal string? TimeoutAt { get; private set; }
    private long deadline;
    private int depth, rootIndex;
    private Move current;

    internal StrategicSearch(StrategicGraph graph, IEnumerable<string> used, bool trace = false, bool measure = false)
    {
        this.graph = graph;
        this.measure = measure;
        if (trace) Trace = new();
        left = Array.ConvertAll(graph.Edges, e => e.Amount);
        totals = (int[])graph.Totals.Clone();
        foreach (string word in used)
            if (word.Length > 0 && graph.EdgeIds.TryGetValue((word[0], word[^1]), out int id) && left[id] > 0) Change(id, -1);
    }

    private Move Name(int id)
    {
        var e = graph.Edges[id];
        return new(graph.Letters[e.First], graph.Letters[e.Last]);
    }

    private void Change(int id, int delta)
    {
        left[id] += delta;
        totals[graph.Edges[id].First] += delta;
    }

    private void Check()
    {
        if (Stopwatch.GetTimestamp() >= deadline) throw new TimeoutException();
    }

    private int Guide(int id, int nextShield, int replies)
    {
        int target = graph.Edges[id].Last;
        return -graph.Hint[Math.Min(nextShield, graph.Hint.Length - 1)][target] * 4 - BitOperations.Log2((uint)replies + 1);
    }

    // node scores always belong to the player who is about to move.
    private int Search(int at, int shield, int remaining, int alpha, int beta, int ply, int extension)
    {
        Visits++;
        Check();
        var moves = frames[ply];
        moves.Clear();
        int nextShield = Math.Max(0, shield - 1), bestHint = -Limit, firstIndex = 0, loops = 0;
        bool exits = false;
        int scanned = 0;
        long tick = measure && remaining == 0 ? Stopwatch.GetTimestamp() : 0;
        try
        {
            foreach (int id in graph.Outgoing[at])
            {
                if ((++scanned & 127) == 0) Check();
                if (left[id] == 0) continue;
                var edge = graph.Edges[id];
                int replies = graph.Capacity(edge, totals);
                if (replies < shield) continue;
                if (replies == 0) return Win - ply - 1;
                if (edge.Last == at) loops += left[id]; else exits = true;
                int guide = Guide(id, nextShield, replies);
                if (guide > bestHint) { firstIndex = moves.Count; bestHint = guide; }
                moves.Add(id);
            }
            if (moves.Count == 0) return -Win + ply;
            // 보호막이 없고 모든 합법 수가 같은 음절을 돌려줄 때에만 압축한다.
            if (shield == 0 && !exits && loops + ply <= 128)
                return loops % 2 == 0 ? -Win + ply + loops : Win - ply - loops;
            if (remaining == 0 && (extension == 0 || moves.Count != 1))
                return bestHint / 4 + Math.Min(12, BitOperations.Log2((uint)moves.Count) * 2);
        }
        finally { if (tick != 0) EvalTicks += Stopwatch.GetTimestamp() - tick; }

        (moves[0], moves[firstIndex]) = (moves[firstIndex], moves[0]);
        int best = -Limit;
        foreach (int id in moves)
        {
            Change(id, -1);
            int score;
            try { score = -Search(graph.Edges[id].Last, nextShield, Math.Max(0, remaining - 1), -beta, -alpha, ply + 1, remaining == 0 ? extension - 1 : extension); }
            finally { Change(id, 1); }
            best = Math.Max(best, score);
            alpha = Math.Max(alpha, score);
            if (alpha >= beta) break;
        }
        return best;
    }

    internal List<Move> FindMoves(char initial, int shield, TimeSpan budget, bool tangPotAvailable = false)
    {
        deadline = Stopwatch.GetTimestamp() + (long)(budget.TotalSeconds * Stopwatch.Frequency);
        BestScore = null; CompletedDepth = SelectedDepth = 0; Visits = EvalTicks = 0; TimeoutAt = null; Trace?.Clear();
        int at = graph.Resolve(initial), nextShield = Math.Max(0, shield - 1);
        var order = new List<int>();
        List<int>? deferred = null;
        int potId = graph.EdgeIds.GetValueOrDefault(('탕', '솥'), -1);
        if (at < 0) return [];
        int chosen = -1, winScore = -Limit, winId = -1, winDepth = 0;
        var lost = new HashSet<int>();
        int lossId = -1, lossScore = -Limit, lossDepth = 0;
        try
        {
            depth = 0;
            foreach (int id in graph.Outgoing[at])
            {
                if (order.Count > 0) Check();
                var edge = graph.Edges[id];
                if (left[id] == 0 || graph.Capacity(edge, totals) < shield) continue;
                if (tangPotAvailable && potId >= 0 && left[potId] > 0 && graph.Letters[edge.Last] == '탕')
                {
                    // 지금 단어까지 소모한 뒤, 상대 턴의 보호막으로 탕솥을 검사한다.
                    var pot = graph.Edges[potId];
                    int replies = graph.Capacity(pot, totals) - (edge.First == pot.Last || edge.First == graph.Doum[pot.Last] ? 1 : 0);
                    if (replies >= nextShield)
                    {
                        (deferred ??= new()).Add(id);
                        continue;
                    }
                }
                order.Add(id);
            }
            if (order.Count == 0 && deferred != null) { order = deferred; deferred = null; }
            if (order.Count == 0) return [];
            // ID is stable under a different SQL/dictionary iteration order.
            order = order.OrderByDescending(id =>
            {
                Check();
                return Guide(id, nextShield, graph.Capacity(graph.Edges[id], totals));
            }).ThenBy(id => id).ToList();
            chosen = order[0];
            for (depth = 1; depth <= MaxDepth; depth++)
            {
                int alpha = -Limit, leader = -1;
                for (rootIndex = 0; rootIndex < order.Count; rootIndex++)
                {
                    int id = order[rootIndex];
                    current = Name(id);
                    // 진단 중에는 동점도 창 안에 포함해 상한과 실제 동점을 구별한다.
                    int window = Trace is null ? -alpha : 1 - alpha;
                    Change(id, -1);
                    int score;
                    try { score = -Search(graph.Edges[id].Last, nextShield, depth - 1, -Limit, window, 1, Forced); }
                    finally { Change(id, 1); }
                    Trace?.Add(new(depth, rootIndex + 1, current, score, score < alpha ? "상한" : score == alpha ? "동점" : "정확"));
                    if (score <= -Win + 128) lost.Add(id);
                    if (score > alpha)
                    {
                        alpha = score; leader = id;
                        if (score <= -Win + 128 && score > lossScore) { lossScore = score; lossId = id; lossDepth = depth; }
                        if (score >= Win - 128 && score > winScore) { winScore = score; winId = id; winDepth = depth; }
                    }
                }
                chosen = leader; BestScore = alpha; SelectedDepth = CompletedDepth = depth;
                order.Remove(leader); order.Insert(0, leader);
                if (Math.Abs(alpha) >= Win - 128 && Win - Math.Abs(alpha) <= depth) break;
            }
        }
        catch (TimeoutException)
        {
            TimeoutAt = depth == 0 ? "최초 후보" : $"깊이 {depth}, 순서 {rootIndex + 1}, {current.First}→{current.Last}";
        }
        if (winId >= 0 && (BestScore is null || BestScore < winScore))
        {
            chosen = winId; BestScore = winScore; SelectedDepth = winDepth;
        }
        else if (lossId >= 0 && lost.Count == order.Count && (BestScore is null || BestScore > -Win + 128))
        {
            // 모든 후보의 패배가 확인됐다. 미완료 반복의 상한 점수는 표시하지 않는다.
            chosen = lossId; BestScore = lossScore; SelectedDepth = lossDepth;
        }
        else if (chosen >= 0 && lost.Contains(chosen) && lost.Count < order.Count)
        {
            chosen = order.First(id => !lost.Contains(id)); BestScore = null; SelectedDepth = 0;
        }
        if (chosen >= 0 && order[0] != chosen) { order.Remove(chosen); order.Insert(0, chosen); }
        if (deferred != null) order.AddRange(deferred);
        return order.ConvertAll(Name);
    }
}
