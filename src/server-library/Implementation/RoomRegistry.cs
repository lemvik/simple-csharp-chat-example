using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Critical.Chat.Protocol;
using Microsoft.Extensions.Logging;

namespace Critical.Chat.Server.Implementation
{
    public class RoomRegistry : IRoomRegistry
    {
        private readonly ILogger<RoomRegistry> logger;
        private readonly ConcurrentDictionary<string, RoomTracker> rooms;
        private readonly SemaphoreSlim roomsLock;
        private readonly CancellationTokenSource lifetime;

        public RoomRegistry(ILogger<RoomRegistry> logger)
        {
            this.logger = logger;
            this.rooms = new ConcurrentDictionary<string, RoomTracker>();
            this.roomsLock = new SemaphoreSlim(1, 1);
            this.lifetime = new CancellationTokenSource();
        }

        public async Task<IServerChatRoom> CreateRoom(string roomName, CancellationToken token = default)
        {
            try
            {
                await roomsLock.WaitAsync(token);

                var existing = rooms.Values.Select(room => room.Room)
                                    .FirstOrDefault(room => room.Name.Equals(roomName));

                if (existing != null)
                {
                    return existing;
                }

                var newRoom = new ServerChatRoom(roomName, roomName);
                var roomLifetime = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
                var tracker = new RoomTracker(newRoom, roomLifetime);
                if (!rooms.TryAdd(newRoom.Id, tracker))
                {
                    throw new Exception($"Failed to create [room={roomName}]");
                }

                tracker.Task = tracker.Room.RunAsync(roomLifetime.Token);
                logger.LogDebug("Created [room={Room}]", newRoom);
                return newRoom;
            }
            finally
            {
                roomsLock.Release();
            }
        }

        public Task<IServerChatRoom> GetRoom(string roomId, CancellationToken token = default)
        {
            return rooms.TryGetValue(roomId, out var room)
                ? Task.FromResult<IServerChatRoom>(room.Room)
                : Task.FromResult<IServerChatRoom>(null);
        }

        public Task<IReadOnlyCollection<IServerChatRoom>> ListRooms()
        {
            var chatRooms = rooms.Values.Select(tracker => tracker.Room).ToList();
            return Task.FromResult<IReadOnlyCollection<IServerChatRoom>>(chatRooms);
        }

        public async Task CloseRoom(IChatRoom room)
        {
            RoomTracker tracker;
            try
            {
                await roomsLock.WaitAsync();
                if (!rooms.TryRemove(room.Id, out tracker))
                {
                    logger.LogWarning("Unable to close non-existing [room={Room}]", room);
                }
            }
            finally
            {
                roomsLock.Release();
            }

            if (tracker != null)
            {
                tracker.RoomLifetime.Cancel();
                await tracker.Task;
            }
        }

        public async Task Close()
        {
            try
            {
                await roomsLock.WaitAsync();

                var trackers = rooms.Values;
                rooms.Clear();
                await Task.WhenAll(trackers.Select(tracker => tracker.Task));
            }
            finally
            {
                roomsLock.Release();
            }
        }

        private class RoomTracker
        {
            public ServerChatRoom Room { get; }
            public Task Task { get; set; }
            public CancellationTokenSource RoomLifetime { get; }

            public RoomTracker(ServerChatRoom room, CancellationTokenSource roomLifetime)
            {
                Room = room;
                RoomLifetime = roomLifetime;
            }
        }
    }
}
