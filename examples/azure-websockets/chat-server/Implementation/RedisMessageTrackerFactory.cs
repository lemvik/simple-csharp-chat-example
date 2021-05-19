using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using StackExchange.Redis;

namespace Lemvik.Example.Chat.Server.Examples.Azure.Implementation
{
    public class RedisMessageTrackerFactory : IMessageTrackerFactory
    {
        private readonly IConnectionMultiplexer multiplexer;

        public RedisMessageTrackerFactory(IConnectionMultiplexer multiplexer)
        {
            this.multiplexer = multiplexer;
        }

        public Task<IMessageTracker> Create(ChatRoom room, CancellationToken token = default)
        {
            return Task.FromResult<IMessageTracker>(new RedisMessageTracker(room, multiplexer.GetDatabase()));
        }
    }
}
