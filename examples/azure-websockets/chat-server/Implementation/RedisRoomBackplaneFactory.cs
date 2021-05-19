using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using StackExchange.Redis;

namespace Lemvik.Example.Chat.Server.Examples.Azure.Implementation
{
    public class RedisRoomBackplaneFactory : IRoomBackplaneFactory
    {
        private readonly IConnectionMultiplexer multiplexer;

        public RedisRoomBackplaneFactory(IConnectionMultiplexer multiplexer)
        {
            this.multiplexer = multiplexer;
        }

        public Task<IRoomBackplane> CreateForRoom(ChatRoom chatRoom, CancellationToken token = default)
        {
            return Task.FromResult<IRoomBackplane>(new RedisRoomBackplane(chatRoom, multiplexer.GetSubscriber()));
        }
    }
}
