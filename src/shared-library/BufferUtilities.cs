using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Lemvik.Example.Chat.Shared
{
    public static class BufferUtilities
    {
        public static async Task<byte[]> ReadLengthPrefixedAsync(this Stream stream,
                                                                 CancellationToken token = default)
        {
            var length = await stream.ReadIntegerAsync(token);
            return await stream.ReadAtLeastAsync(length, token);
        }

        public static async Task WriteLengthPrefixedAsync(this Stream stream,
                                                          byte[] buffer,
                                                          CancellationToken token = default)
        {
            await stream.WriteIntegerAsync(buffer.Length, token);
            await stream.WriteAsync(buffer, 0, buffer.Length, token);
        }
        
        private static async Task<byte[]> ReadAtLeastAsync(this Stream stream,
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

        private static async Task<int> ReadIntegerAsync(this Stream stream, CancellationToken token = default)
        {
            var buffer = await stream.ReadAtLeastAsync(sizeof(int), token);
            var integer = BitConverter.ToInt32(buffer, 0);
            return IPAddress.NetworkToHostOrder(integer);
        }

        private static async Task WriteIntegerAsync(this Stream stream, int integer, CancellationToken token = default)
        {
            var network = IPAddress.HostToNetworkOrder(integer);
            var buffer = BitConverter.GetBytes(network);
            await stream.WriteAsync(buffer, 0, buffer.Length, token);
        }

        public static int ReadNetworkInteger(this byte[] buffer, int offset = 0)
        {
            var integer = BitConverter.ToInt32(buffer, offset);
            return IPAddress.NetworkToHostOrder(integer);
        }

        public static void WriteNetworkInteger(this byte[] buffer, int value, int offset = 0)
        {
            var network = IPAddress.HostToNetworkOrder(value);
            var bytes = BitConverter.GetBytes(network);
            bytes.CopyTo(buffer, offset);
        }
    }
}
