using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using Lemvik.Example.Chat.Protocol.Messages;

namespace Lemvik.Example.Chat.Server.Implementation
{
    public class InMemoryRoomBackplaneFactory : IRoomBackplaneFactory
    {
        private readonly ConcurrentDictionary<string, InMemoryRoomBackplane> backplanes;

        public InMemoryRoomBackplaneFactory()
        {
            this.backplanes = new ConcurrentDictionary<string, InMemoryRoomBackplane>();
        }

        public Task<IRoomBackplane> CreateForRoom(ChatRoom chatRoom, CancellationToken token = default)
        {
            var backplane = backplanes.AddOrUpdate(chatRoom.Id, _ => new InMemoryRoomBackplane(),
                                                   (_, roomBackplane) => roomBackplane);
            return Task.FromResult<IRoomBackplane>(backplane);
        }

        private class InMemoryRoomBackplane : IRoomBackplane
        {
            private readonly Channel<ChatMessage> inMemoryChannel;

            public InMemoryRoomBackplane()
            {
                this.inMemoryChannel = Channel.CreateUnbounded<ChatMessage>();
            }

            public Task AddMessage(ChatMessage message, CancellationToken token = default)
            {
                return inMemoryChannel.Writer.WriteAsync(message, token).AsTask();
            }

            public Task<ChatMessage> ReceiveMessage(CancellationToken token = default)
            {
                return inMemoryChannel.Reader.ReadAsync(token).AsTask();
            }
        }
    }
}
