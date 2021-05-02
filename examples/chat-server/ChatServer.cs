using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol.Messages;
using Lemvik.Example.Chat.Protocol.Transport;
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

        public ChatServer(ILogger<ChatServer> logger,
                          IOptions<ServerConfig> serverConfig,
                          IChatUserIdentityProvider identityProvider,
                          IMessageProtocol messageProtocol,
                          IChatServer chatServer)
        {
            this.logger = logger;
            this.messageProtocol = messageProtocol;
            this.chatServer = chatServer;
            this.identityProvider = identityProvider;
            var listeningConfig = serverConfig.Value.Listening;
            var listeningHost = IPAddress.Parse(listeningConfig.Host);
            var listeningPort = listeningConfig.Port;
            this.listener = new TcpListener(listeningHost, listeningPort);
        }

        private async Task AcceptClients(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Server accepting clients [endpoint={}]", listener.LocalEndpoint);

            listener.Start();

            using (cancellationToken.Register(() => listener.Stop()))
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
                        logger.LogError("Caught generic [error={error}]", error);
                    }
                }
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var serverTask = chatServer.RunAsync(stoppingToken);
            var acceptTask = AcceptClients(stoppingToken);
            return Task.WhenAny(serverTask, acceptTask);
        }
    }
}
