using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Lemvik.Example.Chat.Server.Examples.Azure.Implementation
{
    public class RedisRoomSourceFactory : IRoomSourceFactory
    {
        private readonly IDatabaseAsync database;
        private readonly IMessageTrackerFactory messageTrackerFactory;
        private readonly IRoomBackplaneFactory roomBackplaneFactory;
        private readonly ServerConfig.RoomsConfig config;

        public RedisRoomSourceFactory(IDatabaseAsync database,
                                      IMessageTrackerFactory messageTrackerFactory,
                                      IRoomBackplaneFactory roomBackplaneFactory, 
                                      IOptions<ServerConfig.RoomsConfig> config)
        {
            this.database = database;
            this.messageTrackerFactory = messageTrackerFactory;
            this.roomBackplaneFactory = roomBackplaneFactory;
            this.config = config.Value;
        }

        public Task<IRoomSource> CreateAsync(CancellationToken token)
        {
            return Task.FromResult<IRoomSource>(new RedisRoomSource(database,
                                                                    messageTrackerFactory,
                                                                    roomBackplaneFactory,
                                                                    config.PresenceThreshold,
                                                                    config.RoomSize,
                                                                    config.RoomsKey));
        }
    }
}
