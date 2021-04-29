using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Critical.Chat.Server.Examples.TCP
{
    internal class ChatServer : BackgroundService
    {
        private readonly ILogger<ChatServer> logger;
        private readonly IServerRoomsRegistry roomsRegistry;
        private readonly TcpListener listener;

        public ChatServer(ILogger<ChatServer> logger,
                          IOptions<ServerConfig> serverConfig,
                          IServerRoomsRegistry roomsRegistry)
        {
            this.logger = logger;
            this.roomsRegistry = roomsRegistry;
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
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var client = await listener.AcceptTcpClientAsync();
                        logger.LogDebug("Client connected [client={client}]", client.Client.RemoteEndPoint);
                        client.Close();
                    }
                }
                catch (InvalidOperationException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw;
                }
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return AcceptClients(stoppingToken);
        }
    }
}
