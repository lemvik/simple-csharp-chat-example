using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using StackExchange.Redis;

namespace Lemvik.Example.Chat.Server.Examples.Azure.Implementation
{
    public class RedisRoomBackplaneFactory : IRoomBackplaneFactory
    {
        private readonly ISubscriber subscriber;

        public RedisRoomBackplaneFactory(ISubscriber subscriber)
        {
            this.subscriber = subscriber;
        }

        public Task<IRoomBackplane> CreateForRoom(ChatRoom chatRoom, CancellationToken token = default)
        {
            return Task.FromResult<IRoomBackplane>(new RedisRoomBackplane(chatRoom, subscriber));
        }
    }
}
