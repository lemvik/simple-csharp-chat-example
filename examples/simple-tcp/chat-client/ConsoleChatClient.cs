using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Client.Examples;
using Lemvik.Example.Chat.Protocol.Messages;
using Lemvik.Example.Chat.Protocol.Transport;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lemvik.Example.Chat.Client.Example.TCP
{
    public class ConsoleChatClient : BackgroundService
    {
        private readonly ILogger<ConsoleChatClient> logger;
        private readonly IChatClientFactory clientFactory;
        private readonly IMessageProtocol messageProtocol;
        private readonly IConsoleClient consoleClient;
        private readonly TcpClient client;
        private readonly IPAddress serverAddress;
        private readonly int serverPort;

        public ConsoleChatClient(ILogger<ConsoleChatClient> logger,
                                 IOptions<ClientConfig> clientConfig,
                                 IChatClientFactory clientFactory,
                                 IMessageProtocol messageProtocol, 
                                 IConsoleClient consoleClient)
        {
            this.logger = logger;
            this.clientFactory = clientFactory;
            this.messageProtocol = messageProtocol;
            this.consoleClient = consoleClient;
            this.client = new TcpClient();
            var serverConfig = clientConfig.Value.Server;
            this.serverAddress = IPAddress.Parse(serverConfig.Host);
            this.serverPort = serverConfig.Port;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            client.NoDelay = true;
            await client.ConnectAsync(serverAddress, serverPort, stoppingToken);
            logger.LogDebug("Connected to [server={@Server}]", client.Client.RemoteEndPoint);
            var chatClient =
                clientFactory.CreateClient(new TcpChatTransport(client, messageProtocol));

            var connectionTask = chatClient.RunAsync(stoppingToken);
            var interactionTask = consoleClient.InteractAsync(chatClient, stoppingToken); 

            // In retrospect I could have provided some event or different API to wait for client connection
            await Task.Delay(100, stoppingToken);
            Console.WriteLine($"{chatClient.User.Name} entered the chat");

            await Task.WhenAny(connectionTask, interactionTask);
        }
    }
}
