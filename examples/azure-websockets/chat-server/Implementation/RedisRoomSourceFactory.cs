using System;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Lemvik.Example.Chat.Server.Examples.Azure.Implementation
{
    public class RedisRoomSourceFactory : IRoomSourceFactory
    {
        private readonly IDatabaseAsync database;
        private readonly IMessageTrackerFactory messageTrackerFactory;
        private readonly IRoomBackplaneFactory roomBackplaneFactory;
        private readonly string roomsKey;

        public RedisRoomSourceFactory(IDatabaseAsync database,
                                      IMessageTrackerFactory messageTrackerFactory,
                                      IRoomBackplaneFactory roomBackplaneFactory,
                                      string roomsKey = "roomsList")
        {
            this.database = database;
            this.messageTrackerFactory = messageTrackerFactory;
            this.roomBackplaneFactory = roomBackplaneFactory;
            this.roomsKey = roomsKey;
        }

        public Task<IRoomSource> CreateAsync(CancellationToken token)
        {
            return Task.FromResult<IRoomSource>(new RedisRoomSource(database,
                                                                    messageTrackerFactory,
                                                                    roomBackplaneFactory,
                                                                    TimeSpan.FromSeconds(10),
                                                                    5,
                                                                    roomsKey));
        }
    }
}
