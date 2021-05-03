using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;

namespace Lemvik.Example.Chat.Server.Implementation
{
    public class TransientRoomSource : IRoomSource
    {
        private readonly IMessageTrackerFactory messageTrackerFactory;
        private readonly ConcurrentDictionary<string, IRoom> existingRooms;

        public TransientRoomSource(IMessageTrackerFactory messageTrackerFactory)
        {
            this.messageTrackerFactory = messageTrackerFactory;
            this.existingRooms = new ConcurrentDictionary<string, IRoom>();
        }

        public async Task Initialize(IReadOnlyCollection<ChatRoom> initialRooms, CancellationToken token = default)
        {
            await Task.WhenAll(initialRooms.Select(room => BuildRoom(room, token)));
        }

        private async Task<IRoom> BuildRoom(ChatRoom chatRoom, CancellationToken token = default)
        {
            var messageTracker = await messageTrackerFactory.Create(chatRoom, token);
            var room = new Room(chatRoom, messageTracker);
            return existingRooms.AddOrUpdate(room.ChatRoom.Id, room, (_, existing) => existing);
        }

        public Task<IRoom> BuildRoom(string roomName, CancellationToken token = default)
        {
            var chatRoom = new ChatRoom(Guid.NewGuid().ToString(), roomName);
            return BuildRoom(chatRoom, token);
        }

        public Task<IReadOnlyCollection<IRoom>> ExistingRooms(CancellationToken token = default)
        {
            var rooms = existingRooms.Values.ToArray();
            return Task.FromResult<IReadOnlyCollection<IRoom>>(rooms);
        }
    }
}
