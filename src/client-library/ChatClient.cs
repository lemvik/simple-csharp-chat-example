using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Critical.Chat.Protocol;
using Critical.Chat.Protocol.Messages;
using Critical.Chat.Protocol.Transport;
using Microsoft.Extensions.Logging;

namespace Critical.Chat.Client
{
    internal class ChatClient : IChatClient
    {
        private readonly ILogger<ChatClient> logger;
        private readonly IChatRequestTransport transport;
        private readonly IDictionary<string, ClientChatRoom> rooms;
        private IChatUser assignedUser;

        internal ChatClient(ILogger<ChatClient> logger,
                            IChatTransport transport)
        {
            this.logger = logger;
            this.transport = new ChatRequestTransport(transport);
            this.rooms = new Dictionary<string, ClientChatRoom>();
        }

        public async Task RunAsync(CancellationToken token = default)
        {
            await HandshakeAsync(token);

            logger.LogDebug("Handshake successful [chatUser={ChatUser}]", assignedUser);

            while (!token.IsCancellationRequested)
            {
                var incomingMessage = await transport.Receive(token);
                await DispatchMessage(incomingMessage, token);
            }
        }

        public async Task<IReadOnlyCollection<IChatRoom>> ListRooms(CancellationToken token = default)
        {
            var request = new ListRoomsRequest();

            var response = await transport.Exchange<ListRoomsResponse>(request, token);

            return response.Rooms;
        }

        public async Task<IChatRoom> CreateRoom(string roomName, CancellationToken token = default)
        {
            var request = new CreateRoomRequest(roomName);

            var response = await transport.Exchange<CreateRoomResponse>(request, token);

            return response.Room;
        }

        public async Task<IClientChatRoom> JoinRoom(IChatRoom room, CancellationToken token = default)
        {
            var request = new JoinRoomRequest(room.Id);

            var response = await transport.Exchange<JoinRoomResponse>(request, token);

            var chatRoom = new ClientChatRoom(assignedUser, response.Room, transport);

            rooms.Add(chatRoom.Id, chatRoom);

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
                    assignedUser = handshakeRequest.User;
                    var response = new HandshakeResponse();
                    await transport.Send(response, token);
                    return;
                }

                logger.LogWarning("Ignoring [message={Message}] while waiting for handshake", incomingMessage);
            }
        }

        private async Task DispatchMessage(IMessage message, CancellationToken token = default)
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
        }
    }
}
