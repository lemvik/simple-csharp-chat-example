using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Shared;
using Nito.AsyncEx;

namespace Lemvik.Example.Chat.Server.Implementation
{
    public class RoomRegistry : IRoomRegistry
    {
        private readonly AsyncLazy<IRoomSource> roomSource;
        private readonly AsyncRunnableTracker<string, IRoom> roomsTracker;
        private readonly CancellationTokenSource registryLifetime;

        public RoomRegistry(IRoomSourceFactory roomSourceFactory)
        {
            this.registryLifetime = new CancellationTokenSource();
            this.roomSource =
                new AsyncLazy<IRoomSource>(async () => await roomSourceFactory.CreateAsync(registryLifetime.Token));
            this.roomsTracker = new AsyncRunnableTracker<string, IRoom>(registryLifetime.Token);
        }

        public async Task<IRoom> CreateRoom(string roomName, CancellationToken token = default)
        {
            var source = await roomSource;
            var newRoom = await source.BuildRoom(roomName, token);
            if (!roomsTracker.TryAdd(newRoom.ChatRoom.Id, newRoom))
            {
                throw new Exception($"Failed to create [room={roomName}]");
            }

            return newRoom;
        }

        public async Task<IRoom> GetRoom(string roomId, CancellationToken token = default)
        {
            // Need to make sure we are initialized.
            await roomSource;
            return roomsTracker.TryGet(roomId, out var room) ? room : null;
        }

        public async Task<IReadOnlyCollection<IRoom>> ListRooms()
        {
            await roomSource;
            var chatRooms = roomsTracker.GetAll();
            return chatRooms;
        }

        public async Task RunAsync(CancellationToken token = default)
        {
            using (token.Register(registryLifetime.Cancel))
            {
                var source = await roomSource;
                var existingRooms = await source.ExistingRooms(token);
                foreach (var existingRoom in existingRooms)
                {
                    if (!roomsTracker.TryAdd(existingRoom.ChatRoom.Id, existingRoom))
                    {
                        throw new ChatException($"Failed to register existing [room={existingRoom}]");
                    }
                }

                await token;
                await roomsTracker.StopTracker();
            }
        }
    }
}
