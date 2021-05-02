using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using Lemvik.Example.Chat.Protocol.Messages;
using Lemvik.Example.Chat.Protocol.Transport;

namespace Lemvik.Example.Chat.Client
{
    public class ClientChatRoom : IClientChatRoom
    {
        public ChatRoom Room { get; }
        private readonly ChatUser user;
        private readonly Channel<ChatMessage> messages;
        private readonly IChatExchangeTransport transport;

        public ClientChatRoom(ChatUser user, ChatRoom room, IChatExchangeTransport chatTransport)
        {
            this.user = user;
            this.Room = room;
            this.transport = chatTransport;
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
            var listRequest = new ListUsersRequest(Room);

            var response = await transport.Exchange<ListUsersResponse>(listRequest, token);

            return response.Users;
        }

        public Task SendMessage(string message, CancellationToken token = default)
        {
            var chatMessage = new ChatMessage(user, Room, message);
            return transport.Send(chatMessage, token);
        }

        public async Task<ChatMessage> GetMessage(CancellationToken token = default)
        {
            return await messages.Reader.ReadAsync(token);
        }
    }
}
