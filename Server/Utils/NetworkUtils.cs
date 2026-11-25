using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Server.Utils
{
    internal static class NetworkUtils
    {
        public static IPAddress? GetHamachiIP()
        {
            // 네트워크 인터페이스 중 하마치 선택
            NetworkInterface? adapter = NetworkInterface
                .GetAllNetworkInterfaces()
                .FirstOrDefault(a => a.Name == "Hamachi");

            if (adapter is null) return null;

            // 하마치 내에서 IPv4 조회
            IPAddress? ip = adapter.GetIPProperties()
                .UnicastAddresses
                .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                ?.Address;

            if (ip is null) return null;

            return ip;
        }
    }
}