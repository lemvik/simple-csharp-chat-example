using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Critical.Chat.Protocol;
using Critical.Chat.Protocol.Messages;
using Critical.Chat.Protocol.Transport;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static System.Threading.Tasks.Task;

namespace Critical.Chat.Client.Example.TCP
{
    public class ConsoleChatClient : BackgroundService
    {
        private readonly ILogger<ConsoleChatClient> logger;
        private readonly IChatClientFactory clientFactory;
        private readonly IMessageProtocol messageProtocol;
        private readonly ClientConfig clientConfig;
        private readonly TcpClient client;
        private readonly IPAddress serverAddress;
        private readonly int serverPort;

        public ConsoleChatClient(ILogger<ConsoleChatClient> logger,
                                 IOptions<ClientConfig> clientConfig,
                                 IChatClientFactory clientFactory,
                                 IMessageProtocol messageProtocol)
        {
            this.logger = logger;
            this.clientFactory = clientFactory;
            this.messageProtocol = messageProtocol;
            this.client = new TcpClient();
            this.clientConfig = clientConfig.Value;
            var serverConfig = clientConfig.Value.Server;
            this.serverAddress = IPAddress.Parse(serverConfig.Host);
            this.serverPort = serverConfig.Port;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await client.ConnectAsync(serverAddress, serverPort, stoppingToken);
            logger.LogDebug("Connected to [server={server}]", client.Client.RemoteEndPoint);
            var chatClient =
                clientFactory.CreateClient(new TcpChatTransport(client, messageProtocol), clientConfig.ChatConfig);

            var connectionTask = chatClient.RunAsync(stoppingToken);
            var inputTask = InteractionLoop(chatClient, stoppingToken);

            await WhenAny(connectionTask, inputTask);
        }

        private async Task InteractionLoop(IChatClient chatClient, CancellationToken stoppingToken)
        {
            var rooms = await chatClient.ListRooms(stoppingToken);
            logger.LogDebug("Enumerated [rooms={rooms}]", rooms);

            IChatRoom roomToJoin;
            if (rooms.Count == 0)
            {
                roomToJoin = await chatClient.CreateRoom(clientConfig.ChatConfig.RoomToCreate, stoppingToken);
            }
            else
            {
                roomToJoin = rooms.First();
            }
            
            logger.LogDebug("Joining [room={room}]", roomToJoin);

            var chatRoom = await chatClient.JoinRoom(roomToJoin, stoppingToken);

            var presentMessage = await chatRoom.GetMessage(stoppingToken);
            
            logger.LogDebug("Received [messages={presentMessage}]", presentMessage);
            
            await Delay(1000, stoppingToken);
        }
    }
}
