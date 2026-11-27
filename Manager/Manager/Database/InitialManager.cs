using MySqlConnector;
using Utils;

namespace Manager.Manager.Database
{
    public static class InitialManager
    {
        private static List<char> startChars = [];

        public static async Task StoreStartChars()
        {
            using var conn = DatabaseManager.GetConnection();
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

        public static async Task<bool> IsExistInitial(HashSet<string> usedWord, char initial)
        {
            using var conn = DatabaseManager.GetConnection();
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
            using var conn = DatabaseManager.GetConnection();
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

        public static char GetStartChar()
        {
            return startChars[Random.Shared.Next(startChars.Count)];
        }
    }
}