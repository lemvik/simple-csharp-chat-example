using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;

namespace Lemvik.Example.Chat.Server.Implementation
{
    public class TransientRoomSource : IRoomSource
    {
        private readonly IMessageTrackerFactory messageTrackerFactory;

        public TransientRoomSource(IMessageTrackerFactory messageTrackerFactory)
        {
            this.messageTrackerFactory = messageTrackerFactory;
        }

        public async Task<IRoom> BuildRoom(string roomName, CancellationToken token = default)
        {
            var chatRoom = new ChatRoom(Guid.NewGuid().ToString(), roomName);
            var messageTracker = await messageTrackerFactory.Create(chatRoom, token);
            return new Room(chatRoom, messageTracker);
        }

        public Task<IReadOnlyCollection<IRoom>> ExistingRooms(CancellationToken token = default)
        {
            return Task.FromResult<IReadOnlyCollection<IRoom>>(Array.Empty<IRoom>());
        }
    }
}
