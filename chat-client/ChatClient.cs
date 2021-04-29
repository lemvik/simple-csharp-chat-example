using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Critical.Chat.Client.Example.TCP
{
    public class ChatClient : BackgroundService
    {
        private readonly ILogger<ChatClient> logger;
        private readonly TcpClient client;
        private readonly IPAddress serverAddress;
        private readonly int serverPort;

        public ChatClient(ILogger<ChatClient> logger, IOptions<ClientConfig> clientConfig)
        {
            this.logger = logger;
            this.client = new TcpClient();
            var serverConfig = clientConfig.Value.Server;
            this.serverAddress = IPAddress.Parse(serverConfig.Host);
            this.serverPort = serverConfig.Port;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await client.ConnectAsync(serverAddress, serverPort, stoppingToken);
            logger.LogDebug("Connected to [server={server}]", client.Client.RemoteEndPoint);
        }
    }
}
