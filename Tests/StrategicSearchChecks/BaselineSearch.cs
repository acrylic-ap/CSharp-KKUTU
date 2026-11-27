using System.Diagnostics;
using Utils;

namespace Manager.Manager.Database
{
    // Counts represent interchangeable words with the same first and last syllable.
    internal sealed class BaselineSearch
    {
        internal readonly record struct Move(char First, char Last);
        private const int Win = 1_000_000;
        private readonly Dictionary<char, Dictionary<char, int>> edges;
        private readonly Dictionary<char, int> totals;
        private readonly HashSet<char> kills;
        private readonly HashSet<char> losses;
        private readonly Dictionary<Move, int> usedEdges = new();
        private readonly Dictionary<char, int> usedStarts = new();
        private readonly Stopwatch clock = new();
        private TimeSpan budget;
        private int nodes;
        internal long Visits, EvalTicks;

        internal int? BestScore { get; private set; }
        internal int CompletedDepth { get; private set; }

        internal BaselineSearch(Dictionary<char, Dictionary<char, int>> edges, Dictionary<char, int> totals, HashSet<char> kills, HashSet<char> losses, IEnumerable<string> usedWords)
        {
            this.edges = edges;
            this.totals = totals;
            this.kills = kills;
            this.losses = losses;
            foreach (string word in usedWords)
                if (!string.IsNullOrEmpty(word)) Change(new Move(word[0], word[^1]), 1);
        }

        private void Change(Move move, int delta)
        {
            usedEdges[move] = usedEdges.GetValueOrDefault(move) + delta;
            usedStarts[move.First] = usedStarts.GetValueOrDefault(move.First) + delta;
        }

        private int Remaining(char initial)
        {
            char doum = HangulUtils.GetDoumChar(initial);
            int count = totals.GetValueOrDefault(initial) - usedStarts.GetValueOrDefault(initial);
            if (doum != initial) count += totals.GetValueOrDefault(doum) - usedStarts.GetValueOrDefault(doum);
            return count;
        }

        private List<Move> LegalMoves(char initial, int shield)
        {
            var moves = new List<Move>();
            AddMoves(initial, shield, moves);
            char doum = HangulUtils.GetDoumChar(initial);
            if (doum != initial) AddMoves(doum, shield, moves);
            return moves;
        }

        private void AddMoves(char first, int shield, List<Move> moves)
        {
            if (!edges.TryGetValue(first, out var outgoing)) return;
            foreach (var edge in outgoing)
            {
                var move = new Move(first, edge.Key);
                if (edge.Value <= usedEdges.GetValueOrDefault(move)) continue;
                int remaining = Remaining(move.Last);
                if (first == move.Last || first == HangulUtils.GetDoumChar(move.Last)) remaining--;
                if (remaining >= shield) moves.Add(move);
            }
        }

        // Static classifications are only hints: word use can invalidate them.
        private int StaticEstimate(char initial, int shield)
        {
            if (shield > 0) return 0;
            if (kills.Contains(initial)) return -60;
            if (losses.Contains(initial)) return 60;
            return 0;
        }

        // Re-evaluate a bounded neighbourhood using the current edge counts. Unknown
        // positions fall back to the static hint and are never treated as proven wins.
        private int Estimate(char initial, int shield)
        {
            long tick = Stopwatch.GetTimestamp();
            try {
            int remainingNodes = 96;
            return EstimatePosition(initial, shield, 2, ref remainingNodes);
            } finally { EvalTicks += Stopwatch.GetTimestamp() - tick; }
        }

        private int EstimatePosition(char initial, int shield, int depth, ref int remainingNodes)
        {
            int fallback = StaticEstimate(initial, shield);
            if (clock.IsRunning) CheckTime();
            if (depth == 0 || remainingNodes-- <= 0) return fallback;

            {
                if (shield > 0)
                {
                    var legal = LegalMoves(initial, shield);
                    if (legal.Count == 0) return -180;
                    int best = -200;
                    foreach (var move in legal)
                    {
                        if (remainingNodes <= 0) return fallback;
                        Change(move, 1);
                        try { best = Math.Max(best, -EstimatePosition(move.Last, shield - 1, depth - 1, ref remainingNodes)); }
                        finally { Change(move, -1); }
                    }
                    return best;
                }

                var moves = LegalMoves(initial, 0);
                if (moves.Count == 0) return -180;
                int loopCount = 0;
                int bestUnshielded = -200;
                bool hasExit = false;
                foreach (var move in moves)
                {
                    if (move.Last == initial)
                    {
                        loopCount += edges[move.First][move.Last] - usedEdges.GetValueOrDefault(move);
                        continue;
                    }
                    if (remainingNodes <= 0) return fallback;
                    hasExit = true;
                    Change(move, 1);
                    try
                    {
                        int value = Remaining(move.Last) == 0 ? 180 : -EstimatePosition(move.Last, 0, depth - 1, ref remainingNodes);
                        bestUnshielded = Math.Max(bestUnshielded, value);
                    }
                    finally { Change(move, -1); }
                }
                if (!hasExit) return loopCount % 2 == 1 ? 120 : -120;
                if (loopCount == 0) return Math.Clamp(bestUnshielded, -180, 180);
                // Parity is a weak ordering hint when exits can change the position.
                int odd = Math.Max(bestUnshielded, -bestUnshielded);
                int even = Math.Max(bestUnshielded, -odd);
                return Math.Clamp((loopCount % 2 == 0 ? even : odd) / 2, -90, 90);
            }
        }

        internal List<Move> FindMoves(char initial, int shield, TimeSpan timeLimit)
        {
            budget = timeLimit;
            clock.Start();
            Debug.WriteLine($"전략 탐색: 시작={initial}, 보호막={shield}, 사용 간선={usedEdges.Count}");
            var moves = LegalMoves(initial, shield);
            if (moves.Count == 0) return moves;
            if (clock.Elapsed >= budget) return moves;
            var ordered = moves.OrderBy(move => {
                Change(move, 1);
                int score = StaticEstimate(move.Last, Math.Max(0, shield - 1));
                Change(move, -1);
                return score;
            }).ToList();

            // Check every root move for an immediate win before spending time on deeper search.
            foreach (var move in ordered)
            {
                if (clock.Elapsed >= budget) return ordered;
                Change(move, 1);
                bool win;
                try { win = LegalMoves(move.Last, Math.Max(0, shield - 1)).Count == 0; }
                finally { Change(move, -1); }
                if (win)
                {
                    BestScore = Win - 1;
                    CompletedDepth = 1;
                    return new[] { move }.Concat(ordered.Where(other => other != move)).ToList();
                }
            }

            var provenLosses = new HashSet<Move>();
            for (int depth = 1; depth <= 64 && clock.Elapsed < budget; depth++)
            {
                var scores = new List<(Move Move, int Score)>();
                try
                {
                    int alpha = -Win * 2;
                    foreach (var move in ordered)
                    {
                        CheckTime();
                        Change(move, 1);
                        int score;
                        try { score = -Search(move.Last, Math.Max(0, shield - 1), depth - 1, -Win * 2, -alpha, 1, 2); }
                        finally { Change(move, -1); }
                        Debug.WriteLine($"전략 후보: 깊이={depth}, 간선={move.First}→{move.Last}, 점수={score}, {(score <= alpha ? "상한" : "정확")}");
                        Debug.WriteLine($"전략 후보: 깊이={depth}, 시작={move.First}, 끝={move.Last}, 점수={score}, 판정={(score <= alpha ? "상한" : "정확")}");
                        scores.Add((move, score));
                        if (score <= -Win + 128) provenLosses.Add(move);
                        if (score >= Win - 128)
                        {
                            BestScore = score;
                            CompletedDepth = depth;
                            return new[] { move }.Concat(ordered.Where(other => other != move)).ToList();
                        }
                        alpha = Math.Max(alpha, score);
                    }
                    ordered = scores.OrderByDescending(item => item.Score).Select(item => item.Move).ToList();
                    BestScore = scores.Max(item => item.Score);
                    CompletedDepth = depth;
                    Debug.WriteLine($"전략 탐색 완료: 깊이={depth}, 점수={BestScore}, 노드={nodes}, 시간={clock.ElapsedMilliseconds}ms");
                    if (scores.Max(item => item.Score) >= Win - depth) break;
                }
                catch (TimeoutException)
                {
                    Debug.WriteLine($"전략 시간 초과: 완료 깊이={CompletedDepth}, 진행 깊이={depth}, 평가 후보={scores.Count}/{ordered.Count}, 노드={nodes}, 경과={clock.ElapsedMilliseconds}ms");
                    Debug.WriteLine($"전략 탐색 시간 초과: 완료 깊이={CompletedDepth}, 노드={nodes}, 시간={clock.ElapsedMilliseconds}ms");
                    var previous = ordered[0];
                    if (provenLosses.Count < ordered.Count)
                        ordered = ordered.OrderBy(move => provenLosses.Contains(move)).ToList();
                    if (ordered[0] != previous) { BestScore = null; CompletedDepth = 0; }
                    break;
                }
            }
            return ordered;
        }

        private void CheckTime()
        {
            if ((++nodes & 127) == 0 && clock.Elapsed >= budget) throw new TimeoutException();
        }

        private int Search(char initial, int shield, int depth, int alpha, int beta, int ply, int extensions)
        {
            Visits++;
            CheckTime();
            var moves = LegalMoves(initial, shield);
            if (moves.Count == 0) return -Win + ply;
            if (depth == 0)
            {
                // A move that immediately leaves no response is a fact, not a heuristic.
                if (shield == 0)
                {
                    foreach (var move in moves)
                    {
                        Change(move, 1);
                        bool win;
                        try { win = Remaining(move.Last) == 0; }
                        finally { Change(move, -1); }
                        if (win) return Win - ply - 1;
                    }
                }
                if (extensions == 0 || (shield > 1 && moves.Count > 2) || (shield == 0 && moves.Count > 2))
                    return Estimate(initial, shield);
                depth = 1;
                extensions--;
            }

            moves.Sort((a, b) => StaticEstimate(a.Last, Math.Max(0, shield - 1)).CompareTo(StaticEstimate(b.Last, Math.Max(0, shield - 1))));
            int best = -Win * 2;
            foreach (var move in moves)
            {
                Change(move, 1);
                int score;
                try { score = -Search(move.Last, Math.Max(0, shield - 1), depth - 1, -beta, -alpha, ply + 1, extensions); }
                finally { Change(move, -1); }
                best = Math.Max(best, score);
                alpha = Math.Max(alpha, score);
                if (alpha >= beta) break;
            }
            return best;
        }
    }
}

