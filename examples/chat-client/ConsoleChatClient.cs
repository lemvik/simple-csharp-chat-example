using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Client.Example.TCP.Commands;
using Lemvik.Example.Chat.Protocol;
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

            var commandsReader = new ConsoleCommandsReader(logger);
            var connectionTask = chatClient.RunAsync(stoppingToken);
            var inputTask = commandsReader.RunAsync(stoppingToken);
            var interactionTask = InteractionLoop(commandsReader, chatClient, stoppingToken);

            await Task.WhenAny(connectionTask, inputTask, interactionTask);
        }

        private async Task InteractionLoop(ICommandsSource commandsSource,
                                           IChatClient chatClient,
                                           CancellationToken stoppingToken)
        {
            var knownRooms = new Dictionary<string, ChatRoom>();
            var roomInstances = new Dictionary<string, IRoom>();
            var roomsTasks = new List<Task>();

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var command = await commandsSource.NextCommand(stoppingToken);
                    await DispatchCommand(command, chatClient, knownRooms, roomInstances, roomsTasks, stoppingToken);
                }
            }
            finally
            {
                await Task.WhenAll(roomsTasks);
            }
        }

        private async Task DispatchCommand(ICommand command,
                                           IChatClient chatClient,
                                           IDictionary<string, ChatRoom> knownRooms,
                                           IDictionary<string, IRoom> roomInstances,
                                           ICollection<Task> roomsTasks,
                                           CancellationToken stoppingToken)
        {
            switch (command)
            {
                case JoinRoomCommand joinRoomCommand:
                {
                    var roomName = joinRoomCommand.RoomName;
                    if (!knownRooms.TryGetValue(roomName, out var room))
                    {
                        logger.LogWarning("Cannot join unknown [room={Room}][known={Keys}]",
                                          roomName,
                                          string.Join(",", knownRooms.Keys));

                        return;
                    }

                    roomInstances[roomName] = await chatClient.JoinRoom(room, stoppingToken);

                    logger.LogInformation("Joined [room={Room}]", roomInstances[roomName]);

                    roomsTasks.Add(ListenForMessages(roomInstances[roomName], stoppingToken));

                    break;
                }
                case ListRoomsCommand:
                {
                    var roomsList = await chatClient.ListRooms(stoppingToken);

                    foreach (var chatRoom in roomsList)
                    {
                        if (knownRooms.TryAdd(chatRoom.Name, chatRoom))
                        {
                            logger.LogInformation("Added [room={Room}]", chatRoom);
                        }
                    }

                    break;
                }
                case SendMessageCommand sendMessageCommand:
                {
                    var roomName = sendMessageCommand.RoomName;
                    if (!roomInstances.TryGetValue(roomName, out var room))
                    {
                        logger.LogWarning("Cannot send to unknown [room={Room}][joined={Keys}]",
                                          roomName,
                                          string.Join(",", roomInstances.Keys));
                        return;
                    }

                    await room.SendMessage(sendMessageCommand.Message, stoppingToken);

                    logger.LogInformation("Sent [message={Message}] to [room={Room}]", sendMessageCommand.Message,
                                          room);

                    break;
                }
                default:
                {
                    logger.LogWarning("Unknown [command={Command}]", command);
                    break;
                }
            }
        }

        private async Task ListenForMessages(IRoom room, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var message = await room.GetMessage(token);
                logger.LogInformation("Message in [room={Room}]: {Message}", message.Room.Name, message.Body);
            }
        }
    }
}
