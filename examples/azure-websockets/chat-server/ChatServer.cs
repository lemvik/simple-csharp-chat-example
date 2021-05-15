using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol.Messages;
using Lemvik.Example.Chat.Protocol.Transport;
using Lemvik.Example.Chat.Server.Implementation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lemvik.Example.Chat.Server.Examples.Azure
{
    internal class ChatServer : BackgroundService
    {
        private readonly ILogger<ChatServer> logger;
        private readonly TcpListener listener;
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
            var listeningConfig = serverConfig.Value.Listening;
            var listeningHost = IPAddress.Parse(listeningConfig.Host);
            var listeningPort = listeningConfig.Port;
            this.listener = new TcpListener(listeningHost, listeningPort);
            this.roomSource = transientRoomSource;
        }

        private async Task AcceptClients(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Server accepting clients [endpoint={@Endpoint}]", listener.LocalEndpoint);

            listener.Start();

            await using (cancellationToken.Register(() => listener.Stop()))
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var client = await listener.AcceptTcpClientAsync();
                        logger.LogDebug("Client connected [client={@Client}]", client.Client.RemoteEndPoint);
                        var chatUser = await identityProvider.Identify(client, cancellationToken);
                        var tcpTransport = new TcpChatTransport(client, messageProtocol);
                        await chatServer.AddClientAsync(chatUser, tcpTransport, cancellationToken);
                    }
                    catch (InvalidOperationException)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        throw;
                    }
                    catch (Exception error)
                    {
                        logger.LogError(error, "Caught generic error while accepting clients");
                    }
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // TODO: This is ugly 
            if (roomSource is TransientRoomSource transientRoomSource)
            {
                await transientRoomSource.Initialize(config.PredefinedRooms.Select(room => room.ToRoom()).ToArray(),
                                                     stoppingToken);
            }

            var serverTask = chatServer.RunAsync(stoppingToken);
            var acceptTask = AcceptClients(stoppingToken);
            var registryTask = roomRegistry.RunAsync(stoppingToken);
            await Task.WhenAny(serverTask, acceptTask, registryTask);
        }
    }
}
