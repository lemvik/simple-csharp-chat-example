using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using Lemvik.Example.Chat.Protocol.Messages;
using Lemvik.Example.Chat.Protocol.Transport;
using Microsoft.Extensions.Logging;

namespace Lemvik.Example.Chat.Client.Implementation
{
    internal class ChatClient : IChatClient
    {
        public ChatUser User { get; private set; }
        public CancellationToken ClientCancellation { get; }

        private readonly ILogger<ChatClient> logger;
        private readonly IChatExchangeTransport transport;
        private readonly ConcurrentDictionary<string, Room> rooms;
        private readonly CancellationTokenSource clientLifetime;

        internal ChatClient(ILogger<ChatClient> logger,
                            IChatTransport transport)
        {
            this.logger = logger;
            this.transport = new ChatExchangeTransport(transport);
            this.rooms = new ConcurrentDictionary<string, Room>();
            this.clientLifetime = new CancellationTokenSource();
            this.ClientCancellation = this.clientLifetime.Token;
        }

        public async Task RunAsync(CancellationToken token = default)
        {
            token.Register(clientLifetime.Cancel);
            await HandshakeAsync(token);

            logger.LogDebug("Handshake successful [chatUser={ChatUser}]", User);

            while (!token.IsCancellationRequested)
            {
                var incomingMessage = await transport.Receive(token);
                await DispatchMessage(incomingMessage, token);
            }
        }

        public async Task<IReadOnlyCollection<ChatRoom>> ListRooms(CancellationToken token = default)
        {
            var request = new ListRoomsRequest();

            var response = await transport.Exchange<ListRoomsResponse>(request, token);

            return response.Rooms;
        }

        public async Task<ChatRoom> CreateRoom(string roomName, CancellationToken token = default)
        {
            var request = new CreateRoomRequest(roomName);

            var response = await transport.Exchange<CreateRoomResponse>(request, token);

            return response.Room;
        }

        public async Task<IRoom> JoinRoom(ChatRoom room, CancellationToken token = default)
        {
            var request = new JoinRoomRequest(room.Id);

            var response = await transport.Exchange<JoinRoomResponse>(request, token);

            var chatRoom = new Room(this, response.Room, transport);

            if (!rooms.TryAdd(chatRoom.ChatRoom.Id, chatRoom))
            {
            }

            foreach (var chatMessage in response.Messages)
            {
                if (!chatRoom.ReceiveMessage(chatMessage))
                {
                    logger.LogWarning("Failed to deliver [message={Message}] to chat [room={Room}]",
                                      chatMessage,
                                      chatRoom);
                }
            }

            return chatRoom;
        }

        private async Task HandshakeAsync(CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                var incomingMessage = await transport.Receive(token);
                if (incomingMessage is HandshakeRequest handshakeRequest)
                {
                    User = handshakeRequest.User;
                    await transport.Send(new HandshakeResponse(), token);
                    return;
                }

                logger.LogWarning("Ignoring [message={Message}] while waiting for handshake", incomingMessage);
            }
        }

        private Task DispatchMessage(IMessage message, CancellationToken token = default)
        {
            logger.LogDebug("Dispatching [message={Message}]", message);

            switch (message)
            {
                case ChatMessage chatMessage:
                {
                    var room = chatMessage.Room;
                    if (!(rooms.TryGetValue(room.Id, out var chatRoom) && chatRoom.ReceiveMessage(chatMessage)))
                    {
                        logger.LogWarning("Failed to dispatch chat [message={Message}] to [room={Room}]", 
                                          message,
                                          room);
                    }

                    break;
                }
                default:
                    logger.LogWarning("Unexpected message to be handled [message={Message}]", message);
                    break;
            }
            
            return Task.CompletedTask;
        }

        public void RemoveRoom(Room stoppedRoom)
        {
            rooms.TryRemove(stoppedRoom.ChatRoom.Id, out _);
        }
    }
}
