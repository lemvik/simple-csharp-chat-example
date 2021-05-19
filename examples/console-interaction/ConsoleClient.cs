using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Client.Examples.Commands;
using Lemvik.Example.Chat.Protocol;
using Microsoft.Extensions.Logging;

namespace Lemvik.Example.Chat.Client.Examples
{
    public class ConsoleClient : IConsoleClient
    {
        private readonly ILogger<ConsoleClient> logger;
        private readonly ConsoleCommandsReader commandsSource;

        public ConsoleClient(ILogger<ConsoleClient> logger)
        {
            this.logger = logger;
            this.commandsSource = new ConsoleCommandsReader(logger);
        }

        public Task InteractAsync(IChatClient client, CancellationToken token = default)
        {
            return Task.WhenAll(commandsSource.RunAsync(token), InteractionLoop(client, token));
        }

        private async Task InteractionLoop(IChatClient chatClient,
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

                    logger.LogDebug("Joined [room={Room}]", roomInstances[roomName]);

                    roomsTasks.Add(ListenForMessages(roomInstances[roomName], stoppingToken));

                    Console.WriteLine($"Joined {room}");

                    break;
                }
                case ListRoomsCommand:
                {
                    var roomsList = await chatClient.ListRooms(stoppingToken);

                    foreach (var chatRoom in roomsList)
                    {
                        if (knownRooms.TryAdd(chatRoom.Name, chatRoom))
                        {
                            logger.LogDebug("Added [room={Room}]", chatRoom);
                        }
                    }

                    foreach (var (room, _) in knownRooms)
                    {
                        Console.WriteLine($"{room}");
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

                    logger.LogDebug("Sent [message={Message}] to [room={Room}]", sendMessageCommand.Message, room);

                    break;
                }
                case CreateRoomCommand createRoomCommand:
                {
                    var roomName = createRoomCommand.RoomName;
                    if (roomInstances.TryGetValue(roomName, out _))
                    {
                        logger.LogWarning("Cannot create existing room [room={Room}][joined={Keys}]",
                                          roomName,
                                          string.Join(",", roomInstances.Keys));
                        return;
                    }

                    await chatClient.CreateRoom(roomName, stoppingToken);
                    
                    break;
                }
                case InfoCommand:
                {
                    Console.WriteLine($"Name: {chatClient.User.Name}");
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
                logger.LogDebug("Message in [room={Room}]: {Message}", message.Room.Name, message.Body);
                Console.WriteLine($"[room={message.Room.Name}][user={message.Sender.Name}]: {message.Body}");
            }
        }
    }
}
