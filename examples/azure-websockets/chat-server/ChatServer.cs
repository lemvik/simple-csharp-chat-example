using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol.Messages;
using Lemvik.Example.Chat.Protocol.Transport;
using Lemvik.Example.Chat.Server.Implementation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lemvik.Example.Chat.Server.Examples.Azure
{
    internal class ChatServer : BackgroundService, IWebSocketAcceptor
    {
        private readonly ILogger<ChatServer> logger;
        private readonly IChatUserIdentityProvider identityProvider;
        private readonly IMessageProtocol messageProtocol;
        private readonly IChatServer chatServer;
        private readonly IRoomRegistry roomRegistry;
        private readonly IRoomSource roomSource;
        private readonly ServerConfig config;

        public ChatServer(ILogger<ChatServer> logger,
                          IOptions<ServerConfig> serverConfig,
                          IChatUserIdentityProvider identityProvider,
                          IMessageProtocol messageProtocol,
                          IChatServer chatServer,
                          IRoomRegistry roomRegistry,
                          IRoomSource transientRoomSource)
        {
            this.logger = logger;
            this.config = serverConfig.Value;
            this.messageProtocol = messageProtocol;
            this.chatServer = chatServer;
            this.roomRegistry = roomRegistry;
            this.identityProvider = identityProvider;
            this.roomSource = transientRoomSource;
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
            // This is taken from https://docs.microsoft.com/en-us/aspnet/core/fundamentals/websockets?view=aspnetcore-5.0
            var chatUser = await identityProvider.Identify(socketContext, token);
            var clientExchange =
                await chatServer.AddClientAsync(chatUser, new WebSocketChatTransport(socket, messageProtocol), token);
            await clientExchange;
        }
    }
}
