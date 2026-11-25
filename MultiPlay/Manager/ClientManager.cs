using System.Net.Sockets;

namespace MultiPlay.Manager
{
    public class ClientManager
    {
        public async Task Matching()
        {
            TcpClient client = new();

            using CancellationTokenSource cts = new(3000);

            await client.ConnectAsync("25.31.207.102", 7777, cts.Token);

            MessageBox.Show("서버에 연결되었습니다.");
        }
    }
}
