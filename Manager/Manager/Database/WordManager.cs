using MySqlConnector;
using Utils;

namespace Manager.Manager.Database
{
    public static class WordManager
    {
        public static async Task<bool> IsExistWord(string word)
        {
            using var conn = DatabaseManager.GetConnection();
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
            using var conn = DatabaseManager.GetConnection();
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
    }
}