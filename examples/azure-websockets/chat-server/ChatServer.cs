using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol.Messages;
using Lemvik.Example.Chat.Protocol.Transport;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lemvik.Example.Chat.Server.Examples.Azure
{
    internal class ChatServer : BackgroundService, IWebSocketAcceptor
    {
        private readonly ILogger<ChatServer> logger;
        private readonly IChatUserIdentityProvider identityProvider;
        private readonly IMessageProtocol messageProtocol;
        private readonly IChatServer chatServer;
        private readonly IRoomRegistry roomRegistry;

        public ChatServer(ILogger<ChatServer> logger,
                          IChatUserIdentityProvider identityProvider,
                          IMessageProtocol messageProtocol,
                          IChatServer chatServer,
                          IRoomRegistry roomRegistry)
        {
            this.logger = logger;
            this.messageProtocol = messageProtocol;
            this.chatServer = chatServer;
            this.roomRegistry = roomRegistry;
            this.identityProvider = identityProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var serverTask = chatServer.RunAsync(stoppingToken);
            var registryTask = roomRegistry.RunAsync(stoppingToken);
            await Task.WhenAll(serverTask, registryTask);
        }

        public async Task AcceptWebSocket(HttpContext socketContext, WebSocket socket,
                                          CancellationToken token = default)
        {
            var chatUser = await identityProvider.Identify(socketContext, token);
            var clientExchange =
                await chatServer.AddClientAsync(chatUser, new WebSocketChatTransport(socket, messageProtocol), token);
            await clientExchange;
        }
    }
}
