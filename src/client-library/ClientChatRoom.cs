using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Critical.Chat.Protocol;
using Critical.Chat.Protocol.Messages;
using Critical.Chat.Protocol.Transport;

namespace Critical.Chat.Client
{
    public class ClientChatRoom : IClientChatRoom
    {
        private readonly IChatUser user;
        private readonly IChatRoom room;
        private readonly Channel<IChatMessage> messages;
        private readonly IChatRequestTransport transport;

        public string Id => room.Id;
        public string Name => room.Name;

        public ClientChatRoom(IChatUser user, IChatRoom room, IChatRequestTransport chatTransport)
        {
            this.user = user;
            this.room = room;
            this.transport = chatTransport;
            this.messages = Channel.CreateUnbounded<IChatMessage>(new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false
            });
        }

        public bool ReceiveMessage(IChatMessage message)
        {
            return messages.Writer.TryWrite(message);
        }

        public bool IsActive { get; private set; } = true;

        public async Task<IReadOnlyCollection<IChatUser>> ListUsers(CancellationToken token = default)
        {
            var listRequest = new ListUsersRequest(room);

            var response = await transport.Exchange<ListUsersResponse>(listRequest, token);

            return response.Users;
        }

        public Task SendMessage(string message, CancellationToken token = default)
        {
            var chatMessage = new ChatMessage(user, room, message);
            return transport.Send(chatMessage, token);
        }

        public async Task<IChatMessage> GetMessage(CancellationToken token = default)
        {
            if (!await messages.Reader.WaitToReadAsync(token))
            {
                IsActive = false;
                return null;
            }

            return await messages.Reader.ReadAsync(token);
        }
    }
}
