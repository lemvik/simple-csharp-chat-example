using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Critical.Chat.Protocol;

namespace Critical.Chat.Client
{
    public class ClientChatRoom : IClientChatRoom
    {
        private readonly IChatRoom room;
        private readonly Channel<IChatMessage> messages;

        public string Id => room.Id;
        public string Name => room.Name;

        public ClientChatRoom(IChatRoom room)
        {
            this.room = room;
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

        public Task SendMessage(string message, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
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
