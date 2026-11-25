using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;

namespace Network
{
    public static class Packet
    {
        public static async Task Send
        (
            NetworkStream stream,
            string message,
            CancellationToken token = default
        )
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            byte[] header = new byte[4];

            BinaryPrimitives.WriteInt32BigEndian(header, data.Length);

            await stream.WriteAsync(header, token);
            await stream.WriteAsync(data, token);
        }

        public static async Task<string?> Receive(NetworkStream stream, CancellationToken token = default)
        {
            byte[] header = new byte[4];

            int received = 0;

            while (received < header.Length)
            {
                int length = await stream.ReadAsync(header.AsMemory(received), token);

                if (length == 0)
                {
                    if (received == 0) return null;

                    throw new EndOfStreamException();
                }

                received += length;
            }

            int dataLength = BinaryPrimitives.ReadInt32BigEndian(header);

            if (dataLength < 0 || dataLength > 65536)
                throw new InvalidDataException("잘못된 데이터 길이입니다.");

            byte[] data = new byte[dataLength];

            await stream.ReadExactlyAsync(data);

            return Encoding.UTF8.GetString(data);
        }
    }
}