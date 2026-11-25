using MySqlConnector;
using Utils;

namespace Manager
{
    public static class DatabaseManager
    {
        private static List<char> startChars = [];

        private static readonly string connectionString = new MySqlConnectionStringBuilder
        {
            Server = "25.31.207.102",
            Port = 3306,
            Database = "KKUTU",
            UserID = "kkutu_client",
            Password = "1234"
        }.ConnectionString;

        public static MySqlConnection GetConnection() // 연결 정보를 담은 객체 생성 후 반환
        {
            return new MySqlConnection(connectionString);
        }
        // 반환 시 반드시 using var을 사용!

        public static async Task StoreStartChars()
        {
            using var conn = GetConnection();
            await conn.OpenAsync();

            string sql = """
                SELECT DISTINCT LEFT(word, 1)
                FROM word
                WHERE injeong = 0
            """;

            using var cmd = new MySqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                char word = reader.GetString(0)[0];
                startChars.Add(word);
            }
        }

        public static async Task<bool> IsExistWord(string word)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();

            string sql = """
                SELECT EXISTS (
                    SELECT 1
                    FROM word
                    WHERE word = @word
                    AND injeong = 0
                )
            """;

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@word", word);

            return Convert.ToBoolean(await cmd.ExecuteScalarAsync());
        }

        public static async Task<bool> IsPossibleWord(string word, int shieldTurn, HashSet<string> usedWord)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();

            char initial = word[^1];

            string usedWordString = $"'{string.Join("', '", usedWord)}'";

            string sql = $"""
                SELECT COUNT(*)
                FROM word
                WHERE
                (
                    word LIKE @initial
                    OR word LIKE @doum
                )
                AND word NOT IN ( {usedWordString} )
                AND word != @word
                AND injeong = 0
            """;

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@initial", $"{initial}%");
            cmd.Parameters.AddWithValue("@word", word);
            cmd.Parameters.AddWithValue("@doum", $"{HangulUtils.GetDoumChar(initial)}%");

            return Convert.ToInt32(await cmd.ExecuteScalarAsync()) >= shieldTurn;
        }

        public static async Task<bool> IsExistInitial(HashSet<string> usedWord, char initial)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();

            string usedWordString = $"'{string.Join("', '", usedWord)}'";

            string sql = $"""
                SELECT EXISTS (
                    SELECT 1
                    FROM word
                    WHERE (
                        word LIKE @word
                        OR word LIKE @doum_word
                    )
                    AND word NOT IN ({usedWordString})
                    AND injeong = 0
                )
            """;

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@word", $"{initial}%");
            cmd.Parameters.AddWithValue("@doum_word", $"{HangulUtils.GetDoumChar(initial)}%");

            return Convert.ToBoolean(await cmd.ExecuteScalarAsync());
        }

        public static async Task<Dictionary<char, int>> GetPossibleInitial(HashSet<string> usedWord, char initial)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();

            string usedWordString = $"'{string.Join("', '", usedWord)}'";

            string sql = $"""
                WITH initials AS (
                    SELECT DISTINCT RIGHT(word, 1) AS initial
                    FROM word
                    WHERE (word LIKE @word OR word LIKE @doum_word)
                    AND injeong = 0
                    AND word NOT IN ({usedWordString})
                ),
                possible AS (
                    SELECT initial FROM initials
                    UNION
                    SELECT getDoumCharV2(initial) FROM initials
                ),
                counts AS (
                    SELECT w.initial, COUNT(*) AS amount
                    FROM word w
                    JOIN possible p ON w.initial = p.initial
                    WHERE w.injeong = 0
                    AND w.word NOT IN ({usedWordString})
                    GROUP BY w.initial
                )
                SELECT
                    i.initial,
                    COALESCE(c.amount, 0) + COALESCE(d.amount, 0) AS amount
                FROM initials i
                LEFT JOIN counts c
                    ON i.initial = c.initial
                LEFT JOIN counts d
                    ON getDoumCharV2(i.initial) = d.initial
                    AND i.initial != d.initial
            """;

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@word", $"{initial}%");
            cmd.Parameters.AddWithValue("@doum_word", $"{HangulUtils.GetDoumChar(initial)}%");

            using var reader = await cmd.ExecuteReaderAsync();

            Dictionary<char, int> possibleInitials = [];

            while (await reader.ReadAsync())
                possibleInitials.Add(reader.GetString(0)[0], reader.GetInt32(1));

            return possibleInitials;
        }

        public static async Task<string> FindAvailableRandomWord(HashSet<string> usedWord, char initial, int shieldTurn)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();

            string usedWordString = $"'{string.Join("', '", usedWord)}'";

            // 랜덤 지정
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
                Dictionary<char, int> possibleInitials = await GetPossibleInitial(usedWord, initial);

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
                ORDER BY RAND()
                LIMIT 1;
            """;

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@word", $"{initial}%");
            cmd.Parameters.AddWithValue("@doum_word", $"{HangulUtils.GetDoumChar(initial)}%");

            return Convert.ToString(await cmd.ExecuteScalarAsync()) ?? "";
        }

        public static char GetStartChar()
        {
            return startChars[Random.Shared.Next(startChars.Count)];
        }
    }
}
