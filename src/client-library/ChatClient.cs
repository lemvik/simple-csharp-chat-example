using System;
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
        private readonly IChatTransport transport;
        private readonly IDictionary<ulong, TaskCompletionSource<IMessage>> pendingMessages;
        private readonly IDictionary<string, ClientChatRoom> rooms;
        private ulong sequence;
        private IChatUser assignedUser;

        internal ChatClient(ILogger<ChatClient> logger,
                            IChatTransport transport)
        {
            this.logger = logger;
            this.transport = transport;
            this.pendingMessages = new Dictionary<ulong, TaskCompletionSource<IMessage>>();
            this.rooms = new Dictionary<string, ClientChatRoom>();
        }

        public async Task RunAsync(CancellationToken token = default)
        {
            await HandshakeAsync(token);

            logger.LogDebug("Handshake successful [chatUser={ChatUser}]", assignedUser);

            while (!token.IsCancellationRequested)
            {
                var incomingMessage = await transport.Receive(token);

                if (pendingMessages.TryGetValue(incomingMessage.Id, out var pendingRequest))
                {
                    pendingMessages.Remove(incomingMessage.Id);
                    pendingRequest.SetResult(incomingMessage);
                }
                else
                {
                    await DispatchMessage(incomingMessage, token);
                }
            }
        }

        public async Task<IReadOnlyCollection<IChatRoom>> ListRooms(CancellationToken token = default)
        {
            var request = new ListRoomsRequest(++sequence);

            var response = await Exchange<ListRoomsResponse>(request, token);

            return response.Rooms;
        }

        public async Task<IChatRoom> CreateRoom(string roomName, CancellationToken token = default)
        {
            var request = new CreateRoomRequest(++sequence, roomName);

            var response = await Exchange<CreateRoomResponse>(request, token);

            return response.Room;
        }

        public async Task<IClientChatRoom> JoinRoom(IChatRoom room, CancellationToken token = default)
        {
            var request = new JoinRoomRequest(++sequence, room.Id);

            var response = await Exchange<JoinRoomResponse>(request, token);

            var chatRoom = new ClientChatRoom(response.Room);

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
                    var response = new HandshakeResponse(handshakeRequest.Id);
                    await transport.Send(response, token);
                    return;
                }

                logger.LogWarning("Ignoring [message={Message}] while waiting for handshake", incomingMessage);
            }
        }

        private async Task<TResult> Exchange<TResult>(IMessage message, CancellationToken token = default)
            where TResult : class, IMessage
        {
            var completion = new TaskCompletionSource<IMessage>();
            pendingMessages.Add(message.Id, completion);

            await transport.Send(message, token);

            var response = await completion.Task;

            if (response is TResult result)
            {
                return result;
            }

            throw new Exception($"Received unexpected [response={response}] for [request={message}]");
        }

        private Task DispatchMessage(IMessage message, CancellationToken token = default)
        {
            logger.LogDebug("Dispatching [message={Message}]", message);

            switch (message.Type)
            {
                case MessageType.SendMessage:
                    break;
                case MessageType.ReceiveMessage:
                    break;
                default:
                    logger.LogWarning("Unexpected message to be handled [message={Message}]", message);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
