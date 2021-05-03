using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Shared;
using Microsoft.Extensions.Logging;

namespace Lemvik.Example.Chat.Server.Implementation
{
    public class RoomRegistry : IRoomRegistry, IAsyncRunnable
    {
        private readonly ILogger<RoomRegistry> logger;
        private readonly IRoomSource roomSource;
        private readonly AsyncRunnableTracker<string, IRoom> roomsTracker;
        private readonly CancellationTokenSource registryLifetime;

        public RoomRegistry(ILogger<RoomRegistry> logger, IRoomSource roomSource)
        {
            this.logger = logger;
            this.roomSource = roomSource;
            this.registryLifetime = new CancellationTokenSource();
            this.roomsTracker = new AsyncRunnableTracker<string, IRoom>(registryLifetime.Token);
        }

        public async Task<IRoom> CreateRoom(string roomName, CancellationToken token = default)
        {
            var newRoom = await roomSource.BuildRoom(roomName, token);
            if (!roomsTracker.TryAdd(newRoom.ChatRoom.Id, newRoom))
            {
                throw new Exception($"Failed to create [room={roomName}]");
            }

            return newRoom;
        }

        public Task<IRoom> GetRoom(string roomId, CancellationToken token = default)
        {
            return roomsTracker.TryGet(roomId, out var room)
                ? Task.FromResult(room)
                : Task.FromResult<IRoom>(null);
        }

        public Task<IReadOnlyCollection<IRoom>> ListRooms()
        {
            var chatRooms = roomsTracker.GetAll();
            return Task.FromResult(chatRooms);
        }

        public async Task RunAsync(CancellationToken token = default)
        {
            await token;
            await roomsTracker.StopTracker();
        }
    }
}
