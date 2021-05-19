using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol.Messages;

namespace Lemvik.Example.Chat.Protocol.Transport
{
    public class TcpChatTransport : IChatTransport
    {
        private readonly TcpClient client;
        private readonly IMessageProtocol protocol;

        public TcpChatTransport(TcpClient client, IMessageProtocol protocol)
        {
            this.client = client;
            this.protocol = protocol;
        }

        public Task Send(IMessage message, CancellationToken token = default)
        {
            var stream = client.GetStream();

            return protocol.Serialize(message, stream, token);
        }

        public Task<IMessage> Receive(CancellationToken token = default)
        {
            var stream = client.GetStream();

            return protocol.Parse(stream, token);
        }

        public Task Close()
        {
            client.Close();
            return Task.CompletedTask;
        }
    }
}
