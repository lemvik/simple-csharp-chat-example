using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol.Messages;

namespace Lemvik.Example.Chat.Protocol.Transport
{
    public class WebSocketChatTransport : IChatTransport
    {
        private readonly ClientWebSocket webSocket;
        private readonly IMessageProtocol protocol;
        
        public WebSocketChatTransport(ClientWebSocket webSocket, IMessageProtocol protocol)
        {
            this.webSocket = webSocket;
            this.protocol = protocol;
        }

        public Task Send(IMessage message, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<IMessage> Receive(CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }

        public Task Close()
        {
            return webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Transport closed", CancellationToken.None);
        }
    }
}
