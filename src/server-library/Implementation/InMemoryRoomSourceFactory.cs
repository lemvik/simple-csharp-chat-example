using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;

namespace Lemvik.Example.Chat.Server.Implementation
{
    public class InMemoryRoomSourceFactory : IRoomSourceFactory
    {
        private readonly IMessageTrackerFactory messageTrackerFactory;
        private readonly IRoomBackplaneFactory backplaneFactory;
        private readonly ICollection<ChatRoom> initialRooms;

        public InMemoryRoomSourceFactory(IMessageTrackerFactory messageTrackerFactory,
                                         IRoomBackplaneFactory backplaneFactory, 
                                         ICollection<ChatRoom> initialRooms)
        {
            this.messageTrackerFactory = messageTrackerFactory;
            this.backplaneFactory = backplaneFactory;
            this.initialRooms = initialRooms;
        }

        public async Task<IRoomSource> CreateAsync(CancellationToken token)
        {
            var inMemorySource = new InMemoryRoomSource(messageTrackerFactory, backplaneFactory, initialRooms);
            await inMemorySource.InitializeAsync(token);
            return inMemorySource;
        }
    }
}
