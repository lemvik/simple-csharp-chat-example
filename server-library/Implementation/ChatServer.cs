using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Critical.Chat.Protocol.Messages;
using Critical.Chat.Protocol.Transport;
using Microsoft.Extensions.Logging;

namespace Critical.Chat.Server.Implementation
{
    public class ChatServer : IChatServer
    {
        private readonly ILogger<ChatServer> logger;
        private readonly Channel<(IConnectedClient, IMessage)> messages;
        private readonly ConcurrentDictionary<string, IConnectedClient> clients;

        public ChatServer(ILogger<ChatServer> logger)
        {
            this.logger = logger;
            this.messages = Channel.CreateUnbounded<(IConnectedClient, IMessage)>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
            this.clients = new ConcurrentDictionary<string, IConnectedClient>();
        }

        public async Task RunAsync(CancellationToken token = default)
        {
            logger.LogDebug("Starting chat server main loop.");
            var reader = messages.Reader;
            while (await reader.WaitToReadAsync(token))
            {
                var (client, message) = await reader.ReadAsync(token);
                await DispatchAsync(client, message, token);
            }
            logger.LogDebug("Chat server main loop completed.");
        }

        public Task AddClientAsync(IChatTransport transport, CancellationToken token = default)
        {
            logger.LogDebug("Adding a client [connection={connection}]", transport);
            throw new System.NotImplementedException();
        }

        private Task DispatchAsync(IConnectedClient client, IMessage message, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }
    }
}
