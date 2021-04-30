using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Critical.Chat.Protocol.Protobuf
{
    internal static class BufferUtilities
    {
        internal static async Task<byte[]> ReadLengthPrefixedAsync(this Stream stream,
                                                                   CancellationToken token = default)
        {
            var length = await stream.ReadIntegerAsync(token);
            return await stream.ReadAtLeastAsync(length, token);
        }

        internal static async Task WriteLengthPrefixedAsync(this Stream stream,
                                                            byte[] buffer,
                                                            CancellationToken token = default)
        {
            await stream.WriteIntegerAsync(buffer.Length, token);
            await stream.WriteAsync(buffer, 0, buffer.Length, token);
        }

        internal static async Task<byte[]> ReadAtLeastAsync(this Stream stream,
                                                            int length,
                                                            CancellationToken token = default)
        {
            var buffer = new byte[length];
            var read = 0;
            while (read < length)
            {
                read += await stream.ReadAsync(buffer, read, length - read, token);
            }

            return buffer;
        }

        internal static async Task<int> ReadIntegerAsync(this Stream stream, CancellationToken token = default)
        {
            var buffer = await stream.ReadAtLeastAsync(sizeof(int), token);
            var integer = BitConverter.ToInt32(buffer, 0);
            return IPAddress.NetworkToHostOrder(integer);
        }

        internal static async Task WriteIntegerAsync(this Stream stream, int integer, CancellationToken token = default)
        {
            var network = IPAddress.HostToNetworkOrder(integer);
            var buffer = BitConverter.GetBytes(network);
            await stream.WriteAsync(buffer, 0, buffer.Length, token);
        }
    }
}
