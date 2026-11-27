using MySqlConnector;

namespace Manager.Manager.Database
{
    public static class DatabaseManager
    {
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
    }
}