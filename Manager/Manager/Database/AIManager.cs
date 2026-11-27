using MySqlConnector;
using System.Diagnostics;
using Utils;

namespace Manager.Manager.Database
{
    public static class AIManager
    {
        private static async Task<string> GetAvailableWordSql(HashSet<string> usedWord, char initial, int shieldTurn)
        {
            string usedWordString = $"'{string.Join("', '", usedWord)}'";

            string sql = """
                SELECT word
                FROM word
                WHERE (
                    word LIKE @word
                    OR word LIKE @doum_word
                )
             """;

            if (shieldTurn > 0)
            {
                Dictionary<char, int> possibleInitials = await InitialManager.GetPossibleInitial(usedWord, initial);

                string safeInitials = $"'{string.Join("', '",
                                    possibleInitials
                                        .Where(p => p.Value > shieldTurn)
                                        .Select(p => p.Key))}'";

                string boundaryInitials = $"'{string.Join("', '",
                                                possibleInitials
                                                    .Where(p => p.Value == shieldTurn)
                                                    .Select(p => p.Key))}'";

                sql += $"""

            AND (
                RIGHT(word, 1) IN ({safeInitials})
                OR (
                    RIGHT(word, 1) IN ({boundaryInitials})
                    AND initial != RIGHT(word, 1)
                    AND initial != getDoumCharV2(RIGHT(word, 1))
                )
            )
            """;
            }

            sql += $"""

        AND word NOT IN ({usedWordString})
        AND injeong = 0
        """;

            return sql;
        }

        public static async Task<string> FindRandomWord(HashSet<string> usedWord, char initial, int shieldTurn)
        {
            string sql = await GetAvailableWordSql(usedWord, initial, shieldTurn);

            sql += """

            ORDER BY RAND()
            LIMIT 1;
            """;

            using var conn = DatabaseManager.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@word", $"{initial}%");
            cmd.Parameters.AddWithValue("@doum_word", $"{HangulUtils.GetDoumChar(initial)}%");

            return Convert.ToString(await cmd.ExecuteScalarAsync()) ?? "";
        }

        public static async Task<string> FindTheoryWord(HashSet<string> usedWord, char initial, int shieldTurn)
        {
            string sql = await GetAvailableWordSql(usedWord, initial, shieldTurn);

            sql += $"""

                ORDER BY CASE
                    WHEN RIGHT(word, 1) IN (
                        SELECT initial FROM kill_syllables
                        WHERE reason = '사용 가능한 단어 없음'
                    ) THEN -1

                    WHEN RIGHT(word, 1) IN (
                        SELECT initial FROM kill_syllables
                    ) THEN 0

                    WHEN RIGHT(word, 1) = @initial THEN
                        CASE
                            WHEN (
                                SELECT COUNT(*)
                                FROM word AS loop_word
                                WHERE (
                                    loop_word.word LIKE @word
                                    OR loop_word.word LIKE @doum_word
                                )
                                AND RIGHT(loop_word.word, 1) = @initial
                                AND loop_word.word NOT IN (
                                    '{string.Join("', '", usedWord)}'
                                )
                                AND loop_word.injeong = 0
                            ) % 2 = 1
                            AND @initial IN (
                                SELECT initial FROM lose_syllables
                                WHERE reason LIKE '모든 비돌림 불리,%'
                            ) THEN 0

                            ELSE 2
                        END

                    WHEN initial = @doum_initial
                    AND RIGHT(word, 1) = @doum_initial
                    AND @initial IN (
                        SELECT initial FROM lose_syllables
                        WHERE reason LIKE '두음법칙 돌림 소모:%'
                    )
                    AND (
                        SELECT COUNT(*)
                        FROM word AS loop_word
                        WHERE loop_word.word LIKE @doum_word
                        AND RIGHT(loop_word.word, 1) = @doum_initial
                        AND loop_word.word NOT IN (
                            '{string.Join("', '", usedWord)}'
                        )
                        AND loop_word.injeong = 0
                    ) % 2 = 1 THEN 0

                    WHEN RIGHT(word, 1) IN (
                        SELECT initial FROM lose_syllables
                    ) THEN 2

                    ELSE 1
                END, RAND()
                LIMIT 1;
                """;

            using var conn = DatabaseManager.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@word", $"{initial}%");
            cmd.Parameters.AddWithValue("@doum_word", $"{HangulUtils.GetDoumChar(initial)}%");
            cmd.Parameters.AddWithValue("@initial", initial.ToString());
            cmd.Parameters.AddWithValue("@doum_initial", HangulUtils.GetDoumChar(initial).ToString());

            return Convert.ToString(await cmd.ExecuteScalarAsync()) ?? "";
        }

        public static async Task<string> FindPreferredWord(HashSet<string> usedWord, char initial, int shieldTurn)
        {
            string sql = await GetAvailableWordSql(usedWord, initial, shieldTurn);

            sql += $"""

                ORDER BY CASE
                    WHEN RIGHT(word, 1) IN (
                        SELECT initial FROM kill_syllables
                        WHERE reason = '사용 가능한 단어 없음'
                    ) THEN -1

                    WHEN RIGHT(word, 1) IN (
                        SELECT initial FROM kill_syllables
                    ) THEN 0

                    WHEN RIGHT(word, 1) = @initial THEN
                        CASE
                            WHEN (
                                SELECT COUNT(*)
                                FROM word AS loop_word
                                WHERE (
                                    loop_word.word LIKE @word
                                    OR loop_word.word LIKE @doum_word
                                )
                                AND RIGHT(loop_word.word, 1) = @initial
                                AND loop_word.word NOT IN (
                                    '{string.Join("', '", usedWord)}'
                                )
                                AND loop_word.injeong = 0
                            ) % 2 = 1
                            AND @initial IN (
                                SELECT initial FROM lose_syllables
                                WHERE reason LIKE '모든 비돌림 불리,%'
                            ) THEN 0

                            ELSE 2
                        END

                    WHEN initial = @doum_initial
                    AND RIGHT(word, 1) = @doum_initial
                    AND @initial IN (
                        SELECT initial FROM lose_syllables
                        WHERE reason LIKE '두음법칙 돌림 소모:%'
                    )
                    AND (
                        SELECT COUNT(*)
                        FROM word AS loop_word
                        WHERE loop_word.word LIKE @doum_word
                        AND RIGHT(loop_word.word, 1) = @doum_initial
                        AND loop_word.word NOT IN (
                            '{string.Join("', '", usedWord)}'
                        )
                        AND loop_word.injeong = 0
                    ) % 2 = 1 THEN 0

                    WHEN RIGHT(word, 1) IN (
                        SELECT initial FROM lose_syllables
                    ) THEN 2

                    ELSE 1
                END, RAND()
                LIMIT 1;
                """;

            using var conn = DatabaseManager.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@word", $"{initial}%");
            cmd.Parameters.AddWithValue("@doum_word", $"{HangulUtils.GetDoumChar(initial)}%");
            cmd.Parameters.AddWithValue("@initial", initial.ToString());
            cmd.Parameters.AddWithValue("@doum_initial", HangulUtils.GetDoumChar(initial).ToString());

            return Convert.ToString(await cmd.ExecuteScalarAsync()) ?? "";
        }


        private static readonly Dictionary<char, Dictionary<char, int>> edges = new();
        private static readonly Dictionary<char, int> initialCounts = new();
        private static readonly HashSet<char> killInitials = new();
        private static readonly HashSet<char> loseInitials = new();

        private static readonly SemaphoreSlim cacheLock = new(1, 1);
        private static StrategicGraph? graph;
        private static bool hasTangPot;

        private static async Task LoadStrategicCache()
        {
            await cacheLock.WaitAsync();

            try
            {
                if (graph != null)
                    return;

                edges.Clear();
                initialCounts.Clear();
                killInitials.Clear();
                loseInitials.Clear();

                using var conn = DatabaseManager.GetConnection();
                await conn.OpenAsync();

                using (var cmd = new MySqlCommand("""
                    SELECT LEFT(word,1), RIGHT(word,1), COUNT(*), SUM(CASE WHEN word NOT REGEXP '[^가-힣]' THEN 1 ELSE 0 END)
                    FROM word WHERE injeong = 0 AND CHAR_LENGTH(word) > 0
                    GROUP BY LEFT(word,1), RIGHT(word,1)
                    """, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        char first = reader.GetString(0)[0];
                        char last = reader.GetString(1)[0];
                        int amount = reader.GetInt32(2);

                        if (!edges.ContainsKey(first))
                            edges[first] = new();

                        int playable = reader.GetInt32(3);
                        if (playable > 0) edges[first][last] = playable;
                        initialCounts[first] = initialCounts.GetValueOrDefault(first) + amount;
                    }
                }

                using (var cmd = new MySqlCommand("SELECT initial FROM kill_syllables", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        killInitials.Add(reader.GetString(0)[0]);
                }

                using (var cmd = new MySqlCommand("SELECT initial FROM lose_syllables", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        loseInitials.Add(reader.GetString(0)[0]);
                }

                using (var cmd = new MySqlCommand("SELECT EXISTS(SELECT 1 FROM word WHERE word = '탕솥' AND injeong = 0)", conn))
                    hasTangPot = Convert.ToBoolean(await cmd.ExecuteScalarAsync());
                graph = new StrategicGraph(edges, initialCounts, killInitials, loseInitials);
            }
            finally
            {
                cacheLock.Release();
            }
        }

        public static async Task<(string Word, int? Score, int Depth)> FindStrategicWord(HashSet<string> usedWord, char initial, int shieldTurn)
        {
            var totalClock = Stopwatch.StartNew();
            await LoadStrategicCache();
            long cacheMilliseconds = totalClock.ElapsedMilliseconds;

            string? traceDir = Environment.GetEnvironmentVariable("KKUTU_STRATEGIC_TRACE");
            bool trace = !string.IsNullOrWhiteSpace(traceDir);
            var preparation = Stopwatch.StartNew();
            var search = new StrategicSearch(graph!, usedWord, trace, trace);
            bool tangPotAvailable = hasTangPot && !usedWord.Contains("탕솥");
            var choices = search.FindMoves(initial, shieldTurn, TimeSpan.FromSeconds(3) - preparation.Elapsed, tangPotAvailable);
            long searchMilliseconds = totalClock.ElapsedMilliseconds - cacheMilliseconds;
            void SaveTrace(string word, int index)
            {
                if (!trace) return;
                long lookup = totalClock.ElapsedMilliseconds-cacheMilliseconds-searchMilliseconds;
                try
                {
                    Directory.CreateDirectory(traceDir!);
                    var data = new { Name = "대국", Initial = initial, Shield = shieldTurn, Used = usedWord, Kills = killInitials, Losses = loseInitials,
                        Edges = graph!.Edges.Select(e => new { First = graph.Letters[e.First], Last = graph.Letters[e.Last], e.Amount }),
                        TangPotAvailable = tangPotAvailable, Totals = initialCounts, Candidates = choices, Observations = search.Trace, search.TimeoutAt, search.CompletedDepth, search.Visits,
                        Word = word, CandidateIndex = index, Score = index == 0 ? search.BestScore : null, Depth = index == 0 ? search.SelectedDepth : 0,
                        EvaluationMs = search.EvalTicks * 1000.0 / Stopwatch.Frequency, CacheMs = cacheMilliseconds, SearchMs = searchMilliseconds, LookupMs = lookup };
                    string path = Path.Combine(traceDir!, $"turn-{Guid.NewGuid():N}.json");
                    using var stream = File.Create(path);
                    System.Text.Json.JsonSerializer.Serialize(stream,data);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            if (choices.Count == 0) { SaveTrace("", -1); return ("", null, 0); }

            using var conn = DatabaseManager.GetConnection();
            await conn.OpenAsync();
            using var cmd = new MySqlCommand("SELECT word FROM word WHERE word LIKE CONCAT(@first, '%') AND RIGHT(word, 1) = @last AND injeong = 0", conn);
            var first = cmd.Parameters.Add("@first", MySqlDbType.VarChar);
            var last = cmd.Parameters.Add("@last", MySqlDbType.VarChar);

            for (int i = 0; i < choices.Count; i++)
            {
                var choice = choices[i];
                first.Value = choice.First.ToString();
                last.Value = choice.Last.ToString();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    string word = reader.GetString(0);
                    if (HangulUtils.IsHangulWord(word) && word[0] == choice.First && word[^1] == choice.Last && !usedWord.Contains(word))
                    {
                        SaveTrace(word, i);
                        return (word, i == 0 ? search.BestScore : null, i == 0 ? search.SelectedDepth : 0);
                    }
                }
                Debug.WriteLine($"전략 후보 단어 없음: 시작={choice.First}, 끝={choice.Last}, 후보순위={i + 1}");
            }

            SaveTrace("", -1);
            return ("", null, 0);
        }
    }
}
