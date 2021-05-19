using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol.Messages;
using Lemvik.Example.Chat.Protocol.Transport;
using Lemvik.Example.Chat.Shared;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lemvik.Example.Chat.Server.Examples.TCP
{
    internal class ChatServer : BackgroundService
    {
        private readonly ILogger<ChatServer> logger;
        private readonly TcpListener listener;
        private readonly IChatUserIdentityProvider identityProvider;
        private readonly IMessageProtocol messageProtocol;
        private readonly IChatServer chatServer;
        private readonly IRoomRegistry roomRegistry;
        private readonly TaskTracker tracker;

        public ChatServer(ILogger<ChatServer> logger,
                          IOptions<ServerConfig> serverConfig,
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
            var listeningConfig = serverConfig.Value.Listening;
            var listeningHost = IPAddress.Parse(listeningConfig.Host);
            var listeningPort = listeningConfig.Port;
            this.listener = new TcpListener(listeningHost, listeningPort);
            this.tracker = new TaskTracker();
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
                        tracker.Run(await chatServer.AddClientAsync(chatUser, tcpTransport, cancellationToken),
                                    cancellationToken);
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
            var serverTask = chatServer.RunAsync(stoppingToken);
            var acceptTask = AcceptClients(stoppingToken);
            var registryTask = roomRegistry.RunAsync(stoppingToken);
            await Task.WhenAny(serverTask, acceptTask, registryTask);
        }
    }
}
