using System;
using System.Buffers;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol.Messages;
using Lemvik.Example.Chat.Shared;

namespace Lemvik.Example.Chat.Protocol.Transport
{
    public class WebSocketChatTransport : IChatTransport
    {
        private const int ReadBufferSize = 4096;
        private readonly WebSocket webSocket;
        private readonly IMessageProtocol protocol;
        private readonly ArrayPool<byte> arrayPool;

        public WebSocketChatTransport(WebSocket webSocket, IMessageProtocol protocol)
        {
            this.webSocket = webSocket;
            this.protocol = protocol;
            this.arrayPool = ArrayPool<byte>.Shared;
        }

        public async Task Send(IMessage message, CancellationToken token = default)
        {
            var outStream = new MemoryStream();
            await protocol.Serialize(message, outStream, token);
            var outBuffer = new ArraySegment<byte>(outStream.GetBuffer(), 0, (int) outStream.Length);
            await webSocket.SendAsync(outBuffer, WebSocketMessageType.Binary, true, token);
        }

        public async Task<IMessage> Receive(CancellationToken token = default)
        {
            var incomingStream = await PumpIncomingMessage(token);
            return await protocol.Parse(incomingStream, token);
        }

        public Task Close()
        {
            return webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Transport closed", CancellationToken.None);
        }

        private async Task<MemoryStream> PumpIncomingMessage(CancellationToken token = default)
        {
            var outStream = new MemoryStream();
            var readBuffer = this.arrayPool.Rent(ReadBufferSize);

            try
            {
                var eom = false;

                while (!eom)
                {
                    var result =
                        await webSocket.ReceiveAsync(new ArraySegment<byte>(readBuffer, 0, ReadBufferSize), token);
                    eom = result.EndOfMessage;
                    await outStream.WriteAsync(readBuffer, 0, result.Count, token);
                }

                outStream.Position = 0;
                return outStream;
            }
            finally
            {
                this.arrayPool.Return(readBuffer);
            }
        }
    }
}
