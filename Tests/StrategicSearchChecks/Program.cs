using System.Diagnostics;
using System.Text.Json;
using Manager.Manager.Database;

const int Mate = 1_000_000;
var fixtures = new List<Position>();
var rng = new Random(732);
for (int i = 0; i < 180; i++)
{
    var words = Enumerable.Range(0, rng.Next(3, 11)).Select(_ => new Pair("가나여녀"[rng.Next(4)], "가나여녀"[rng.Next(4)])).ToArray();
    fixtures.Add(Position.Make("무작위" + i, words, "가나여녀"[i % 4], i % 3));
}
var cycle = Position.Make("순환", [new('벽', '널'), new('널', '벽'), new('벽', '벽'), new('벽', '닥'), new('닥', '닥'), new('닥', '샘'), new('샘', '샘')], '벽', 0);
fixtures.AddRange([cycle, cycle with { Used = ["벽널", "널벽"] }, cycle with { Used = ["닥닥", "샘창자샘"] }]);
fixtures.Add(Position.Make("패배 지연", [new('가', '나'), new('나', '다'), new('가', '마'), new('마', '바'), new('바', '사'), new('사', '아')], '가', 0));
fixtures.Add(Position.Make("보호막 해제와 우회", [new('가', '나'), new('나', '다'), new('나', '라'), new('다', '마'), new('라', '바'), new('바', '사')], '가', 1));
fixtures.Add(Position.Make("공유 단어와 경계", [new('녀', '여'), new('여', '녀'), new('여', '녀'), new('여', '다')], '녀', 2) with { Used = ["여녀"] });
// 제시된 수순의 단어만 있는 보호막 0의 부분 사전. 전체 대국의 다른 출구는 알 수 없다.
string[] tangLine = ["죽력탕", "탕솥", "솥뚜껑", "껑껑", "껑뚱", "뚱싯", "싯가락", "낙지저냐", "냐짱", "짱짱", "짱깨", "깨꾼"];
var tang = Position.Make("탕 수순", tangLine.Select(w => new Pair(w[0], w[^1])), '죽', 0);
fixtures.Add(tang);
foreach (int n in new[] { 1, 3, 9 })
    fixtures.Add(tang with { Initial = tangLine[n-1][^1], Used = tangLine[..n] });
fixtures.Add(tang with { Initial = '뚱', Used = ["죽력탕", "탕솥", "솥뚜껑", "껑뚱"] });
// 인공 출구는 받는 쪽이 바로 꾼을 줄 수 있다. 탕솥만 조사하면 되는
// 공격자와 달리, 탕을 패배로 판정하려면 이 출구도 하나씩 배제해야 한다.
var distractions = "가거고구기새소수".SelectMany(c => new[] { new Edge('탕', c, 1), new Edge(c, '꾼', 1) });
var crowdedTang = tang with { Name = "탕의 여러 대응", Edges = [.. tang.Edges, .. distractions] };
// 솥에서 나가던 두 간선은 사전에 남아 있지만 마지막 단어까지 사용했다.
var depletedPot = crowdedTang with { Name = "솥 출구 소모", Edges = [.. crowdedTang.Edges, new('솥','바',1), new('솥','새',1)], Used = ["솥바","솥새"] };
var depletedChoice = depletedPot with { Name = "소모 후 죽의 두 후보", Edges = [.. depletedPot.Edges, new('죽','솥',1)] };
fixtures.AddRange([depletedPot, depletedChoice, depletedChoice with { Used = [..depletedChoice.Used, "껑껑"] }]);
var alternative = crowdedTang with { Name = "죽의 두 후보", Edges = [.. crowdedTang.Edges, new('죽','솥',1)] };
fixtures.AddRange([crowdedTang, alternative, alternative with { Edges = alternative.Edges.Reverse().ToArray() },
    alternative with { Used = ["껑껑"] }, alternative with { Shield = 2 }]);
var timing = new List<Position> { cycle };
var forced = Enumerable.Range(0, 25).Select(i => new Pair((char)(44032 + i), (char)(44033 + i))).ToArray();
fixtures.Add(Position.Make("강제", forced, '가', 0));
timing.Add(fixtures[^1]);
var wide = (from i in Enumerable.Range(0, 120) from j in Enumerable.Range(0, 120) where rng.Next(4) == 0 || i == 0 select new Pair((char)(44032 + i), (char)(44032 + j))).ToArray();
timing.Add(Position.Make("넓은초반", wide, '가', 12));
timing.Add(timing[^1] with { Name = "해제직전", Shield = 1 });
// 두 갈래 모두 보호막 중에는 충분한 단어가 있다. 마지막 수의 주인은 경로 길이로 독립 판정한다.
var corridor = new List<Edge>();
for (int branch = 0; branch < 2; branch++)
{
    char Node(int layer, int lane) => (char)(0xAC00 + branch * 256 + layer * 12 + lane);
    int length = branch == 0 ? 14 : 13;
    for (int level = 1; level <= length; level++)
        for (int lane = 0; lane < 8; lane++)
            if (level == 1) corridor.Add(new('가', Node(level, lane), 1));
            else for (int prev = 0; prev < 8; prev++) corridor.Add(new(Node(level - 1, prev), Node(level, lane), 12));
}
var fork = new Position("보호막 출구", corridor.ToArray(), '가', 12, [], [], []);
timing.Add(fork);
if (args.Contains("--replay"))
    timing = [JsonSerializer.Deserialize<Position>(File.ReadAllText(args[Array.IndexOf(args, "--replay") + 1]))!];
bool baseline = args.Contains("--baseline");
var cache = new Dictionary<Position, StrategicGraph>();
Result Run(Position p, int ms, bool? legacy = null)
{
    var edges = p.Edges.GroupBy(e => e.First).ToDictionary(g => g.Key, g => g.ToDictionary(e => e.Last, e => e.Amount));
    var totals = p.Totals ?? edges.ToDictionary(e => e.Key, e => e.Value.Values.Sum());
    if (!(legacy ?? baseline)) return RunNew(p, edges, ms);
    var clock = Stopwatch.StartNew();
    var search = new BaselineSearch(edges, totals, p.Kills.ToHashSet(), p.Losses.ToHashSet(), p.Used);
    var moves = search.FindMoves(p.Initial, p.Shield, TimeSpan.FromMilliseconds(ms));
    return new(moves.Select(m => new Pair(m.First, m.Last)).ToArray(), search.BestScore, search.CompletedDepth, clock.Elapsed.TotalMilliseconds, search.Visits, search.EvalTicks * 1000.0 / Stopwatch.Frequency);
}
int choiceErrors = 0, proofErrors = 0, legalErrors = 0, unknown = 0, distanceErrors = 0;
foreach (var p in fixtures)
{
    var tokens = p.Edges.SelectMany(e => Enumerable.Repeat(new Pair(e.First, e.Last), e.Amount)).ToList();
    foreach (var word in p.Used) tokens.Remove(new(word[0], word[^1]));
    var truth = Solve(tokens, p.Initial, p.Shield, 0);
    var result = Run(p, 700);
    if (!truth.Keys.ToHashSet().SetEquals(result.Moves)) legalErrors++;
    int best = truth.Count == 0 ? -Mate : truth.Values.Max();
    bool wrongChoice = result.Moves.Length > 0 && truth.GetValueOrDefault(result.Moves[0]) != best;
    if (wrongChoice) { choiceErrors++; Console.WriteLine($"선택 오차 {p.Name} {JsonSerializer.Serialize(p)}"); }
    if (result.Score is int s && Math.Abs(s) > 900000)
    {
        if (Math.Sign(s) != Math.Sign(best)) proofErrors++;
        if (result.Moves.Length > 0 && s != truth.GetValueOrDefault(result.Moves[0])) distanceErrors++;
    }
    else if (result.Moves.Length > 0) unknown++;
}
Console.WriteLine($"국면={fixtures.Count} 합법오류={legalErrors} 선택오류={choiceErrors} 증명오류={proofErrors} 거리오류={distanceErrors} 미확정={unknown}");
if (!baseline)
{
    // 탕솥 자체와 같은 간선에 속한 다른 단어를 구분한다. 각 후보의 실제 탐색 여부도 확인한다.
    var edges = new Dictionary<char, Dictionary<char, int>>
    {
        ['죽'] = new() { ['탕'] = 1, ['가'] = 1 }, ['탕'] = new() { ['솥'] = 2, ['끝'] = 2 },
        ['솥'] = new() { ['끝'] = 1 }, ['가'] = new() { ['끝'] = 4 }
    };
    var graph = new StrategicGraph(edges, edges.ToDictionary(e => e.Key, e => e.Value.Values.Sum()), [], []);
    foreach (var (shield, available, used, defer) in new (int, bool, string[], bool)[]
        { (0,true,[],true), (2,true,[],true), (3,true,[],false), (0,false,["탕솥"],false), (0,false,[],false), (0,true,["죽가"],false) })
    {
        var engine = new StrategicSearch(graph, used, trace: true);
        var choices = engine.FindMoves('죽', shield, TimeSpan.FromMilliseconds(700), available);
        bool searched = engine.Trace!.Any(o => o.Edge.Last == '탕');
        if (choices.Count == 0 || searched == defer || (defer && choices[0].Last == '탕'))
        {
            Console.WriteLine($"탕 예외 오류: 보호막={shield}, 사용={string.Join(',',used)}, 탕솥 가능={available}");
            Environment.ExitCode = 1;
        }
    }
}
if (!baseline && legalErrors + choiceErrors + proofErrors + distanceErrors + unknown != 0) Environment.ExitCode = 1;
foreach (var p in timing)
{
    var setup = Stopwatch.StartNew();
    Run(p, 0);
    Console.WriteLine($"압축 입력 최초 준비 및 0ms 호출 {p.Name}={setup.Elapsed.TotalMilliseconds:F3}ms");
    Run(p, 700);
    var results = Enumerable.Range(0, 8).Select(_ => Run(p, 700)).ToArray();
    var sorted = results.OrderBy(r => r.Ms).ToArray();
    Console.WriteLine($"{p.Name} 후보={results[0].Moves.Length} 선택={string.Join('/', results.Select(r => r.Moves.FirstOrDefault()).Distinct())} 점수={string.Join('/', results.Select(r => r.Score).Distinct())} 평균={results.Average(r => r.Ms):F2} P95={sorted[(int)Math.Ceiling(sorted.Length * .95) - 1].Ms:F2} 최대={sorted[^1].Ms:F2} 깊이={string.Join(',', results.Select(r => r.Depth))} 노드/초={results.Sum(r => r.Nodes) * 1000 / results.Sum(r => r.Ms):F0} 평가ms={results.Average(r => r.EvalMs):F2} 기타ms={results.Average(r => r.Ms - r.EvalMs):F2}");
}
static Dictionary<Pair, int> Solve(List<Pair> words, char initial, int shield, int ply)
{
    // 시험 사전에 등장하는 두음 쌍을 직접 명시한다. 실제 단어를 하나씩 제거한다.
    bool Starts(char from, char at) => from == at || (at == '녀' && from == '여') || (at == '라' && from == '나') || (at == '락' && from == '낙');
    var result = new Dictionary<Pair, int>();
    for (int i = 0; i < words.Count; i++)
    {
        var m = words[i];
        if (!Starts(m.First, initial)) continue;
        var left = new List<Pair>(words); left.RemoveAt(i);
        if (left.Count(w => Starts(w.First, m.Last)) < shield) continue;
        var replies = Solve(left, m.Last, Math.Max(0, shield - 1), ply + 1);
        result[m] = replies.Count == 0 ? Mate - ply - 1 : -replies.Values.Max();
    }
    return result;
}
Result RunNew(Position p, Dictionary<char, Dictionary<char, int>> edges, int ms)
{
    if (!cache.TryGetValue(p, out var graph))
        cache[p] = graph = new StrategicGraph(edges, p.Totals ?? edges.ToDictionary(x => x.Key, x => x.Value.Values.Sum()), p.Kills.ToHashSet(), p.Losses.ToHashSet());
    var clock = Stopwatch.StartNew();
    var search = new StrategicSearch(graph, p.Used, measure: true);
    var moves = search.FindMoves(p.Initial, p.Shield, TimeSpan.FromMilliseconds(ms) - clock.Elapsed, p.TangPotAvailable);
    return new(moves.Select(m => new Pair(m.First, m.Last)).ToArray(), search.BestScore, search.CompletedDepth, clock.Elapsed.TotalMilliseconds, search.Visits, search.EvalTicks * 1000.0 / Stopwatch.Frequency);
}
if (!args.Contains("--replay"))
{
    foreach (var test in new[] { fork, fork with { Edges = fork.Edges.Reverse().ToArray() } })
    {
        var r = Run(test, 700);
        bool correct = r.Moves[0].Last >= 0xAD00;
        Console.WriteLine($"보호막 출구 순서 반전 검사: 선택={r.Moves[0]} 정답={correct}");
        if (!baseline && !correct) Environment.ExitCode = 1;
    }
}
if (!baseline)
{
    // 넓은 위치의 두 번째 반복 도중 중단해 실제 가상 간선과 출발 개수가 모두 복원되는지 확인한다.
    var probe = timing.FirstOrDefault(p => p.Name == "넓은초반");
    if (probe != null)
    {
        var engine = new StrategicSearch(cache[probe], probe.Used, true);
        int[] State(string field) => (int[])typeof(StrategicSearch).GetField(field, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(engine)!;
        var counts = State("left").ToArray();
        var totals = State("totals").ToArray();
        engine.FindMoves(probe.Initial, probe.Shield, TimeSpan.FromMilliseconds(25));
        bool restored = engine.TimeoutAt != null && engine.Trace!.Count > 0 && counts.SequenceEqual(State("left")) && totals.SequenceEqual(State("totals"));
        Console.WriteLine($"시간 초과 복원={restored} 중단={engine.TimeoutAt}");
        if (!restored) Environment.ExitCode = 1;
    }
}
if (!baseline && args.Contains("--replay"))
{
    var p = timing.Single();
    var search = new StrategicSearch(cache[p], p.Used, trace: true);
    var order = search.FindMoves(p.Initial, p.Shield, TimeSpan.FromMilliseconds(700), p.TangPotAvailable);
    foreach (var item in search.Trace!) Console.WriteLine(item);
    Console.WriteLine($"선택={order.FirstOrDefault()} 점수={search.BestScore} 완료 깊이={search.CompletedDepth} 중단={search.TimeoutAt}");
}
if (args.Contains("--arena"))
{
    int victories = 0, games = 0;
    foreach (var start in new[] { fork, cycle, fixtures.First(p => p.Name == "패배 지연"), fixtures.First(p => p.Name == "보호막 해제와 우회") })
    {
        for (int seat = 0; seat < 2; seat++)
        {
            var p = start;
            int turn = 0;
            while (true)
            {
                bool old = turn % 2 != seat;
                var r = Run(p, 700, old);
                if (r.Moves.Length == 0) { if (old) victories++; break; }
                var m = r.Moves[0];
                string word = $"{m.First}{turn}{m.Last}";
                p = p with { Initial = m.Last, Shield = Math.Max(0, p.Shield - 1), Used = [.. p.Used, word] };
                turn++;
            }
            games++;
        }
    }
    Console.WriteLine($"교대 대전 신형 승리={victories}/{games}");
}
if (args.Contains("--db"))
{
    foreach (var item in new[] { ('업', 9, new[] { "을지문덕", "덕업", "업업" }), ('춤', 6, new[] { "법고춤" }), ('벽', 0, new[] { "벽널", "널벽" }) })
    {
        var state = item.Item3.ToHashSet();
        var actual = await AIManager.FindStrategicWord(state, item.Item1, item.Item2);
        bool valid = actual.Word.Length > 0 && !state.Contains(actual.Word) && (actual.Word[0] == item.Item1 || actual.Word[0] == Utils.HangulUtils.GetDoumChar(item.Item1)) && await WordManager.IsExistWord(actual.Word) && await WordManager.IsPossibleWord(actual.Word, item.Item2, state);
        Console.WriteLine($"부분 국면 {item.Item1} {actual} 승인={valid}");
        if (!valid) Environment.ExitCode = 1;
    }
}
record struct Pair(char First, char Last);
record Edge(char First, char Last, int Amount);
record Position(string Name, Edge[] Edges, char Initial, int Shield, string[] Used, char[] Kills, char[] Losses)
{
    public bool TangPotAvailable { get; init; }
    public Dictionary<char, int>? Totals { get; init; }
    internal static Position Make(string name, IEnumerable<Pair> pairs, char at, int shield) => new(name, pairs.GroupBy(m => m).Select(g => new Edge(g.Key.First, g.Key.Last, g.Count())).ToArray(), at, shield, [], [], []);
}
record Result(Pair[] Moves, int? Score, int Depth, double Ms, long Nodes, double EvalMs);
