using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol.Messages;
using Lemvik.Example.Chat.Protocol.Transport;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lemvik.Example.Chat.Client.Examples.Azure
{
    public class ConsoleChatClient : BackgroundService
    {
        private readonly ILogger<ConsoleChatClient> logger;
        private readonly IChatClientFactory clientFactory;
        private readonly IMessageProtocol messageProtocol;
        private readonly string remoteUrl;

        public ConsoleChatClient(ILogger<ConsoleChatClient> logger,
                                 IOptions<ClientConfig> clientConfig,
                                 IChatClientFactory clientFactory,
                                 IMessageProtocol messageProtocol)
        {
            this.logger = logger;
            this.clientFactory = clientFactory;
            this.messageProtocol = messageProtocol;
            this.remoteUrl = clientConfig.Value.Server.Url;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var wsClient = await Connect(stoppingToken);
            logger.LogInformation("Connected to [remote={Remote}]", remoteUrl);
            var chatClient = clientFactory.CreateClient(new WebSocketChatTransport(wsClient, messageProtocol));

            await chatClient.RunAsync(stoppingToken);
        }

        private async Task<ClientWebSocket> Connect(CancellationToken token)
        {
            var client = new ClientWebSocket();

            logger.LogInformation("Connecting to [remote={Remote}]", new Uri(remoteUrl));
            await client.ConnectAsync(new Uri(remoteUrl), token);

            return client;
        }
    }
}
