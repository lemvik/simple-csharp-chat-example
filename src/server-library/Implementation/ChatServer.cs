using System;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using Lemvik.Example.Chat.Protocol.Messages;
using Lemvik.Example.Chat.Protocol.Transport;
using Lemvik.Example.Chat.Shared;
using Microsoft.Extensions.Logging;

namespace Lemvik.Example.Chat.Server.Implementation
{
    public class ChatServer : IChatServer
    {
        private readonly ILogger<ChatServer> logger;
        private readonly ILoggerFactory loggerFactory;
        private readonly Channel<(Client, IMessage)> messages;
        private readonly IRoomRegistry roomsRegistry;
        private readonly CancellationTokenSource lifetime;
        private readonly AsyncRunnableTracker<string, ClientRunnable> clientTracker;

        public ChatServer(ILoggerFactory loggerFactory, IRoomRegistry roomsRegistry)
        {
            this.logger = loggerFactory.CreateLogger<ChatServer>();
            this.loggerFactory = loggerFactory;
            this.roomsRegistry = roomsRegistry;
            this.messages = Channel.CreateUnbounded<(Client, IMessage)>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
            this.lifetime = new CancellationTokenSource();
            this.clientTracker = new AsyncRunnableTracker<string, ClientRunnable>(lifetime.Token);
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

        public async Task AddClientAsync(ChatUser chatUser,
                                         IChatTransport transport,
                                         CancellationToken clientToken = default)
        {
            var token = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token, clientToken).Token;
            try
            {
                logger.LogDebug("Handshaking [client={ChatUser}][connection={Connection}]", chatUser, transport);
                var connectedClient = new Client(loggerFactory.CreateLogger<Client>(),
                                                 chatUser,
                                                 transport,
                                                 messages.Writer);
                var handshakeComplete = new TaskCompletionSource<bool>();
                var runnable = new ClientRunnable(handshakeComplete.Task, token, connectedClient, this);
                if (!clientTracker.TryAdd(chatUser.Id, runnable))
                {
                    var exception = new ChatException($"Unable to add [client={chatUser}], one is already connected");
                    handshakeComplete.SetException(exception);
                    throw exception;
                }

                try
                {
                    await HandshakeAsync(chatUser, transport, token);
                    handshakeComplete.SetResult(true);
                }
                catch (Exception reason)
                {
                    clientTracker.TryRemoveAndStop(chatUser.Id, out var clientTask);
                    try
                    {
                        await clientTask;
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    finally
                    {
                        throw reason;
                    }
                }

                logger.LogDebug("Added [client={ChatUser}][connection={Connection}]",
                                chatUser,
                                transport);
            }
            catch (Exception error)
            {
                logger.LogError(error, "Failed to handshake with the [client={ChatUser}]", chatUser);
                throw;
            }
        }

        private static async Task HandshakeAsync(ChatUser chatUser,
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

        private async Task DispatchAsync(Client client,
                                         IMessage message,
                                         CancellationToken token = default)
        {
            if (message is ExchangeMessage exchangeMessage)
            {
                switch (exchangeMessage.Message)
                {
                    case ListRoomsRequest:
                    {
                        var rooms = await roomsRegistry.ListRooms();
                        var chatRooms = rooms.Select(room => room.ChatRoom).ToList();
                        var response = exchangeMessage.MakeResponse(new ListRoomsResponse(chatRooms));
                        await client.SendMessage(response, token);
                        break;
                    }
                    case CreateRoomRequest createRoomRequest:
                    {
                        var room = await roomsRegistry.CreateRoom(createRoomRequest.RoomName, token);
                        var response =
                            exchangeMessage.MakeResponse(new CreateRoomResponse(room.ChatRoom));
                        await client.SendMessage(response, token);
                        break;
                    }
                    case JoinRoomRequest joinRoomRequest:
                    {
                        var room = await roomsRegistry.GetRoom(joinRoomRequest.RoomId, token);
                        await room.AddUser(client, token);
                        client.EnterRoom(room);
                        var mostRecentMessages = await room.MostRecentMessages(5, token);
                        var response =
                            exchangeMessage.MakeResponse(new JoinRoomResponse(room.ChatRoom,
                                                                              mostRecentMessages));
                        await client.SendMessage(response, token);
                        break;
                    }
                    case LeaveRoomRequest leaveRoomRequest:
                    {
                        var room = await roomsRegistry.GetRoom(leaveRoomRequest.Room.Id, token);
                        await room.RemoveUser(client, token);
                        client.LeaveRoom(room);
                        var response = exchangeMessage.MakeResponse(new LeaveRoomResponse(room.ChatRoom));
                        await client.SendMessage(response, token);
                        break;
                    }
                    default:
                        logger.LogError("Unknown [message={Message}]", exchangeMessage.Message);
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private async Task RunClient(Client client, CancellationToken token = default)
        {
            try
            {
                await client.RunAsync(token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception error)
            {
                logger.LogError(error, "Encountered an error running [client={Client}] task", client);
            }
            finally
            {
                logger.LogDebug("Cleaning up client resources [client={Client}]", client);
                clientTracker.TryRemoveAndStop(client.User.Id, out _);

                var clientRooms = client.Rooms;
                // ReSharper disable once MethodSupportsCancellation
                await Task.WhenAll(clientRooms.Select(room => room.RemoveUser(client)));

                logger.LogDebug("Client task completed for [client={Client}]", client);
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

        private Task CleanUp()
        {
            return clientTracker.StopTracker();
        }

        private class ClientRunnable : IAsyncRunnable
        {
            private readonly CancellationToken clientLifetime;
            private readonly Task preRun;
            private readonly Client client;
            private readonly ChatServer server;

            public ClientRunnable(Task preRun, CancellationToken clientLifetime, Client client, ChatServer server)
            {
                this.preRun = preRun;
                this.clientLifetime = clientLifetime;
                this.client = client;
                this.server = server;
            }

            public async Task RunAsync(CancellationToken token = default)
            {
                await preRun;
                var operationToken = CancellationTokenSource.CreateLinkedTokenSource(clientLifetime, token).Token;
                await server.RunClient(client, operationToken);
            }
        }
    }
}
