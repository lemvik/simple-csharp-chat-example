using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Lemvik.Example.Chat.Server.Examples.Azure.Implementation
{
    public class RedisRoomSourceFactory : IRoomSourceFactory
    {
        private readonly IConnectionMultiplexer multiplexer;
        private readonly IMessageTrackerFactory messageTrackerFactory;
        private readonly IRoomBackplaneFactory roomBackplaneFactory;
        private readonly string roomsKey;

        public RedisRoomSourceFactory(IConnectionMultiplexer multiplexer,
                                      IMessageTrackerFactory messageTrackerFactory,
                                      IRoomBackplaneFactory roomBackplaneFactory,
                                      string roomsKey = "roomsList")
        {
            this.multiplexer = multiplexer;
            this.messageTrackerFactory = messageTrackerFactory;
            this.roomBackplaneFactory = roomBackplaneFactory;
            this.roomsKey = roomsKey;
        }

        public Task<IRoomSource> CreateAsync(CancellationToken token)
        {
            return Task.FromResult<IRoomSource>(new RedisRoomSource(multiplexer.GetDatabase(),
                                                                    messageTrackerFactory,
                                                                    roomBackplaneFactory,
                                                                    roomsKey));
        }
    }
}
