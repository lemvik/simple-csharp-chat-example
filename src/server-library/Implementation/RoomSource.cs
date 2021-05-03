using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;

namespace Lemvik.Example.Chat.Server.Implementation
{
    public class RoomSource : IRoomSource
    {
        public Task<IRoom> BuildRoom(string roomName, CancellationToken token = default)
        {
            var serverRoom = new Room(new ChatRoom(Guid.NewGuid().ToString(), roomName));
            return Task.FromResult<IRoom>(serverRoom);
        }

        public Task<IReadOnlyCollection<IRoom>> ExistingRooms(CancellationToken token = default)
        {
            return Task.FromResult<IReadOnlyCollection<IRoom>>(Array.Empty<IRoom>());
        }
    }
}
