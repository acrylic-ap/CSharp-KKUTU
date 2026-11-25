using MultiPlay.Model;
using Server.Utils;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    internal class Server
    {
        private List<Player> players = [];

        static async Task Main(string[] args)
        {
            IPAddress? IP = NetworkUtils.GetHamachiIP();

            if(IP is null)
            {
                Console.WriteLine("하마치를 찾지 못했습니다. 연결이 원활한지 확인 후 재실행해 주세요.");
                return;
            }

            TcpListener listener = new(IP, 7777);

            listener.Start();

            Console.WriteLine($"서버가 시작되었습니다. ({IP}:7777)");

            while (true)
            {
                using TcpClient client = await listener.AcceptTcpClientAsync();

                Console.WriteLine($"클라이언트 접속: {client.Client.RemoteEndPoint}");

                NetworkStream stream = client.GetStream();

                byte[] data = Encoding.UTF8.GetBytes("Hello, Server!");

                await stream.WriteAsync(data);
            }

            listener.Stop();
        }
    }
}
