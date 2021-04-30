using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Critical.Chat.Protocol;
using Critical.Chat.Protocol.Messages;
using Critical.Chat.Protocol.Transport;

namespace Critical.Chat.Server.Implementation
{
    public class ConnectedClient : IConnectedClient
    {
        public IChatUser User { get; }
        private readonly IChatTransport transport;

        public ConnectedClient(IChatUser user, IChatTransport transport)
        {
            User = user;
            this.transport = transport;
        }

        public async Task RunAsync(ChannelWriter<(IConnectedClient, IMessage)> sink, CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                var message = await transport.Receive(token);
                await sink.WriteAsync((this, message), token);
            }
        }

        public Task SendMessage(IMessage message, CancellationToken token = default)
        {
            return transport.Send(message, token);
        }
    }
}
