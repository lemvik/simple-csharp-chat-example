using System;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<string, IConnectedClient> clients;
        private readonly IServerRoomsRegistry roomsRegistry;

        public ChatServer(ILogger<ChatServer> logger, IServerRoomsRegistry roomsRegistry)
        {
            this.logger = logger;
            this.roomsRegistry = roomsRegistry;
            this.messages = Channel.CreateUnbounded<(IConnectedClient, IMessage)>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
            this.clients = new ConcurrentDictionary<string, IConnectedClient>();
        }

        public async Task RunAsync(CancellationToken token = default)
        {
            logger.LogDebug("Starting chat server main loop");
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

            logger.LogDebug("Chat server main loop completed");
        }

        public async Task AddClientAsync(IChatUser chatUser,
                                         IChatTransport transport,
                                         CancellationToken token = default)
        {
            logger.LogDebug("Adding a [client={ChatUser}][connection={Connection}]", chatUser, transport);

            var connectedClient = new ConnectedClient(chatUser, transport);
            if (!clients.TryAdd(chatUser.Id, connectedClient))
            {
                throw new Exception($"Unable to add [client={chatUser}], one is already connected");
            }

            try
            {
                await HandshakeAsync(chatUser, transport, token);

                RunClient(connectedClient, token);
            }
            catch (Exception error)
            {
                logger.LogError(error, "Failed to handshake with the [client={ChatUser}]", chatUser);
            }
        }

        private async Task HandshakeAsync(IChatUser chatUser,
                                          IChatTransport transport,
                                          CancellationToken token = default)
        {
            var handshake = new HandshakeRequest(0, chatUser);
            await transport.Send(handshake, token);
            var response = await transport.Receive(token);
            if (response is HandshakeResponse)
            {
                return;
            }

            throw new Exception($"Expected to receive handshake response [received={response}]");
        }

        private async Task DispatchAsync(IConnectedClient client, IMessage message, CancellationToken token = default)
        {
            switch (message.Type)
            {
                case MessageType.ListRoomsRequest:
                {
                    var rooms = await roomsRegistry.ListRooms();
                    var response = new ListRoomsResponse(message.Id, rooms);
                    await client.SendMessage(response, token);
                    break;
                }
                case MessageType.CreateRoomRequest:
                {
                    var createRoomRequest = message.Cast<CreateRoomRequest>();
                    var room = await roomsRegistry.CreateRoom(createRoomRequest.RoomName);
                    var response = new CreateRoomResponse(createRoomRequest.Id, room);
                    await client.SendMessage(response, token);
                    break;
                }
                case MessageType.JoinRoomRequest:
                {
                    var joinRoomRequest = message.Cast<JoinRoomRequest>();
                    var room = await roomsRegistry.GetRoom(joinRoomRequest.RoomId);
                    await room.AddUser(client, token);
                    var mostRecentMessages = await room.MostRecentMessages(5, token);
                    var response = new JoinRoomResponse(message.Id, room, mostRecentMessages);
                    await client.SendMessage(response, token);
                    break;
                }
                case MessageType.LeaveRoomRequest:
                {
                    var leaveRoomRequest = message.Cast<LeaveRoomRequest>();
                    var room = await roomsRegistry.GetRoom(leaveRoomRequest.Room.Id);
                    await room.RemoveUser(client, token);
                    var response = new LeaveRoomResponse(message.Id, room);
                    await client.SendMessage(response, token);
                    break;
                }
                case MessageType.SendMessage:
                    break;
                case MessageType.ReceiveMessage:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RunClient(IConnectedClient client, CancellationToken token = default)
        {
            client.RunAsync(messages.Writer, token).ContinueWith(result =>
            {
                if (result.IsFaulted)
                {
                    logger.LogError(result.Exception, "Encountered an error in [client={Client}] interaction", client);
                }
                else if (result.IsCanceled)
                {
                    logger.LogDebug("Client stopped due to cancellation [client={Client}]", client);
                }
                else
                {
                    logger.LogDebug("Client terminated loop [client={Client}]", client);
                }

                if (!clients.TryRemove(client.User.Id, out _))
                {
                    logger.LogError("Failed to remove client from clients list [client={Client}]", client);
                }
            }, token);
        }
    }
}
