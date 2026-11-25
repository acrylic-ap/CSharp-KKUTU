using System.Net;
using System.Net.Sockets;

namespace Server.Network
{
    internal class TCPServer
    {
        private readonly TcpListener listener;

        public TCPServer(IPAddress ip, int port)
        {
            listener = new(ip, port);
        }

        public void Start()
        {
            listener.Start();
        }

        public void Stop()
        {
            listener.Stop();
        }
    }
}
