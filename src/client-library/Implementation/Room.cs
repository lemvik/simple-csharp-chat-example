using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using Lemvik.Example.Chat.Protocol.Messages;
using Lemvik.Example.Chat.Protocol.Transport;

namespace Lemvik.Example.Chat.Client.Implementation
{
    internal class Room : IRoom
    {
        public ChatRoom ChatRoom { get; }
        private readonly ChatClient client;
        private readonly Channel<ChatMessage> messages;
        private readonly IChatExchangeTransport transport;
        private readonly CancellationTokenSource roomLifetime;

        public Room(ChatClient client, ChatRoom room, IChatExchangeTransport chatTransport)
        {
            this.client = client;
            this.ChatRoom = room;
            this.transport = chatTransport;
            this.roomLifetime = CancellationTokenSource.CreateLinkedTokenSource(client.ClientCancellation);
            this.messages = Channel.CreateUnbounded<ChatMessage>(new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false
            });
        }

        public bool ReceiveMessage(ChatMessage message)
        {
            return messages.Writer.TryWrite(message);
        }

        public async Task<IReadOnlyCollection<ChatUser>> ListUsers(CancellationToken token = default)
        {
            var operationToken = CancellationTokenSource.CreateLinkedTokenSource(roomLifetime.Token, token).Token;

            var listRequest = new ListUsersRequest(ChatRoom);

            var response = await transport.Exchange<ListUsersResponse>(listRequest, operationToken);

            return response.Users;
        }

        public Task SendMessage(string message, CancellationToken token = default)
        {
            var operationToken = CancellationTokenSource.CreateLinkedTokenSource(roomLifetime.Token, token).Token;

            var chatMessage = new ChatMessage(client.User, ChatRoom, message);
            return transport.Send(chatMessage, operationToken);
        }

        public async Task<ChatMessage> GetMessage(CancellationToken token = default)
        {
            var operationToken = CancellationTokenSource.CreateLinkedTokenSource(roomLifetime.Token, token).Token;

            return await messages.Reader.ReadAsync(operationToken);
        }

        public async Task Leave(CancellationToken token = default)
        {
            var leaveRequest = new LeaveRoomRequest(ChatRoom);
            try
            {
                await transport.Exchange<LeaveRoomResponse>(leaveRequest, token);
            }
            finally
            {
                this.roomLifetime.Cancel();
                this.client.RemoveRoom(this);
                this.messages.Writer.Complete();
            }
        }

        public override string ToString()
        {
            return $"Room[ChatRoom={ChatRoom},Client={client}]";
        }
    }
}
