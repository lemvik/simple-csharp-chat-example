using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Critical.Chat.Protocol;
using Critical.Chat.Protocol.Messages;
using Critical.Chat.Protocol.Transport;
using Microsoft.Extensions.Logging;

namespace Critical.Chat.Server.Implementation
{
    public class ChatServer : IChatServer
    {
        private readonly ILogger<ChatServer> logger;
        private readonly Channel<(IConnectedClient, IMessage)> messages;
        private readonly SemaphoreSlim clientsLock;
        private readonly ConcurrentDictionary<string, ClientTask> clientTasks;
        private readonly IRoomRegistry roomsRegistry;
        private readonly CancellationTokenSource lifetime;

        public ChatServer(ILogger<ChatServer> logger, IRoomRegistry roomsRegistry)
        {
            this.logger = logger;
            this.roomsRegistry = roomsRegistry;
            this.messages = Channel.CreateUnbounded<(IConnectedClient, IMessage)>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
            this.clientsLock = new SemaphoreSlim(1, 1);
            this.clientTasks = new ConcurrentDictionary<string, ClientTask>();
            this.lifetime = new CancellationTokenSource();
        }

        public async Task RunAsync(CancellationToken token = default)
        {
            logger.LogDebug("Starting chat server main loop");
            using (token.Register(lifetime.Cancel))
            {
                try
                {
                    await PumpMessages(lifetime.Token);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception error)
                {
                    logger.LogError(error, "Server loop encountered error");
                }
                finally
                {
                    await CleanUp();
                }

                logger.LogDebug("Chat server main loop completed");
            }
        }

        public async Task AddClientAsync(IChatUser chatUser,
                                         IChatTransport transport,
                                         CancellationToken clientToken = default)
        {
            var token = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token, clientToken).Token;
            try
            {
                await clientsLock.WaitAsync(token);

                logger.LogDebug("Handshaking [client={ChatUser}][connection={Connection}]", chatUser, transport);
                var connectedClient = new ConnectedClient(chatUser, transport, messages.Writer);
                var clientTask = new ClientTask();

                // If we fail to add client we should not run it's `RunAsync` method, so first add, then fire the task.
                if (!clientTasks.TryAdd(chatUser.Id, clientTask))
                {
                    throw new Exception($"Unable to add [client={chatUser}], one is already connected");
                }

                try
                {
                    await HandshakeAsync(chatUser, transport, token);
                }
                catch (Exception)
                {
                    clientTasks.TryRemove(chatUser.Id, out _);
                    throw;
                }

                logger.LogDebug("Handshake successful, adding [client={ChatUser}][connection={Connection}]",
                                chatUser,
                                transport);

                clientTask.Task = RunClient(connectedClient, token);
            }
            catch (Exception error)
            {
                logger.LogError(error, "Failed to handshake with the [client={ChatUser}]", chatUser);
            }
            finally
            {
                clientsLock.Release();
            }
        }

        private async Task HandshakeAsync(IChatUser chatUser,
                                          IChatTransport transport,
                                          CancellationToken token = default)
        {
            var handshake = new HandshakeRequest(chatUser);
            await transport.Send(handshake, token);
            var response = await transport.Receive(token);
            if (response is HandshakeResponse)
            {
                return;
            }

            throw new Exception($"Expected to receive handshake response [received={response}]");
        }

        private async Task DispatchAsync(IConnectedClient client,
                                         IMessage message,
                                         CancellationToken token = default)
        {
            if (message is ExchangeMessage exchangeMessage)
            {
                switch (exchangeMessage.Message)
                {
                    case ListRoomsRequest _:
                    {
                        var rooms = await roomsRegistry.ListRooms();
                        var response = exchangeMessage.MakeResponse(new ListRoomsResponse(rooms));
                        await client.SendMessage(response, token);
                        break;
                    }
                    case CreateRoomRequest createRoomRequest:
                    {
                        var room = await roomsRegistry.CreateRoom(createRoomRequest.RoomName, token);
                        var response =
                            exchangeMessage.MakeResponse(new CreateRoomResponse(room));
                        await client.SendMessage(response, token);
                        break;
                    }
                    case JoinRoomRequest joinRoomRequest:
                    {
                        var room = await roomsRegistry.GetRoom(joinRoomRequest.RoomId, token);
                        await room.AddUser(client, token);
                        var mostRecentMessages = await room.MostRecentMessages(5, token);
                        var response =
                            exchangeMessage.MakeResponse(new JoinRoomResponse(room,
                                                                              mostRecentMessages));
                        await client.SendMessage(response, token);
                        break;
                    }
                    case LeaveRoomRequest leaveRoomRequest:
                    {
                        var room = await roomsRegistry.GetRoom(leaveRoomRequest.Room.Id, token);
                        await room.RemoveUser(client, token);
                        var response = exchangeMessage.MakeResponse(new LeaveRoomResponse(room));
                        await client.SendMessage(response, token);
                        break;
                    }
                    default:
                        logger.LogError("Unknown [message={Message}]", exchangeMessage.Message);
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private async Task RunClient(IConnectedClient client, CancellationToken token = default)
        {
            try
            {
                await client.RunAsync(token);
            }
            catch (Exception error)
            {
                logger.LogError(error, "Encountered an error running [client={Client}] task", client);
            }
            finally
            {
                if (!clientTasks.TryRemove(client.User.Id, out _))
                {
                    logger.LogError("Unable to remove [client={Client}] from server", client);
                }
            }
        }

        private async Task PumpMessages(CancellationToken token = default)
        {
            var reader = messages.Reader;
            while (await reader.WaitToReadAsync(token))
            {
                var (client, message) = await reader.ReadAsync(token);
                try
                {
                    await DispatchAsync(client, message, token);
                }
                catch (Exception error)
                {
                    logger.LogError("Encountered [error={Error}] trying to dispatch [message={Message}]",
                                    error,
                                    message);
                }
            }
        }

        private async Task CleanUp()
        {
            try
            {
                await clientsLock.WaitAsync();

                var tasks = clientTasks.Values.Select(task => task.Task);

                await Task.WhenAll(tasks);
            }
            finally
            {
                clientsLock.Release();
            }
        }

        private class ClientTask
        {
            public Task Task { get; set; }
        }
    }
}
