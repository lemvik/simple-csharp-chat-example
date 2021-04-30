using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Critical.Chat.Protocol.Messages;
using Critical.Chat.Protocol.Transport;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Critical.Chat.Server.Examples.TCP
{
    internal class ChatServer : BackgroundService
    {
        private readonly ILogger<ChatServer> logger;
        private readonly TcpListener listener;
        private readonly IMessageProtocol messageProtocol;
        private readonly IChatServer chatServer;

        public ChatServer(ILogger<ChatServer> logger,
                          IOptions<ServerConfig> serverConfig,
                          IMessageProtocol messageProtocol,
                          IChatServer chatServer)
        {
            this.logger = logger;
            this.messageProtocol = messageProtocol;
            this.chatServer = chatServer;
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
                        logger.LogDebug("Client connected [client={client}]", client.Client.RemoteEndPoint);
                        var tcpTransport = new TcpChatTransport(client, messageProtocol);
                        await chatServer.AddClientAsync(tcpTransport, cancellationToken);
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
