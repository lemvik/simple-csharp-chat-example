using System;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<string, CancellationToken> clientHandles;

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
            this.clientHandles = new ConcurrentDictionary<string, CancellationToken>();
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
                    this.lifetime.Cancel();
                }

                logger.LogDebug("Chat server main loop completed");
            }
        }

        public async Task<Task> AddClientAsync(ChatUser chatUser,
                                               IChatTransport transport,
                                               CancellationToken clientToken = default)
        {
            var token = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token, clientToken).Token;
            try
            {
                if (!clientHandles.TryAdd(chatUser.Id, token))
                {
                    throw new ChatException("Cannot add already added client");
                }

                logger.LogDebug("Handshaking [client={ChatUser}][connection={Connection}]", chatUser, transport);
                var connectedClient = new Client(loggerFactory.CreateLogger<Client>(),
                                                 chatUser,
                                                 transport,
                                                 messages.Writer);

                await HandshakeAsync(chatUser, transport, token);

                logger.LogDebug("Added [client={ChatUser}][connection={Connection}]",
                                chatUser,
                                transport);

                return RunClient(connectedClient, token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception error)
            {
                logger.LogError(error, "Failed to operate [client={ChatUser}]", chatUser);
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
                        if (!await room.AddUser(client, token))
                        {
                            var error = new ChatErrorResponse($"Unable to enter [chatRoom={joinRoomRequest.RoomId}]");
                            var errorResponse = exchangeMessage.MakeResponse(error);
                            await client.SendMessage(errorResponse, token);
                            break;
                        }

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
                clientHandles.TryRemove(client.User.Id, out _);
                logger.LogDebug("Cleaning up client resources [client={Client}]", client);
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
    }
}
