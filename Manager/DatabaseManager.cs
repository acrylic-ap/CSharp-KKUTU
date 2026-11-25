using MySqlConnector;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
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
                        LEFT(word, 1) = @word
                        OR LEFT(word, 1) = @doum_word
                    )
                    AND word NOT IN ({usedWordString})
                    AND injeong = 0
                )
            """;

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@word", initial);
            cmd.Parameters.AddWithValue("@doum_word", HangulUtils.GetDoumChar(initial));

            return Convert.ToBoolean(await cmd.ExecuteScalarAsync());
        }

        public static async Task<string> FindAvailableRandomWord(HashSet<string> usedWord, char initial)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();

            string usedWordString = $"'{string.Join("', '", usedWord)}'";

            string sql = $"""
                SELECT word
                FROM word
                WHERE 
                (
                    LEFT(word, 1) = @word
                    OR LEFT(word, 1) = @doum_word
                )
                AND word NOT IN ( {usedWordString} )
                AND injeong = 0
                ORDER BY RAND()
                LIMIT 1;
            """;

            using var cmd = new MySqlCommand(sql, conn);
            
            cmd.Parameters.AddWithValue("@word", initial);
            cmd.Parameters.AddWithValue("@doum_word", HangulUtils.GetDoumChar(initial));

            Debug.WriteLine(cmd.CommandText);

            using var reader = await cmd.ExecuteReaderAsync();

            await reader.ReadAsync();

            string word = reader.GetString(0);

            return word;
        }

        public static char GetStartChar()
        {
            return startChars[Random.Shared.Next(startChars.Count)];
        }
    }
}
