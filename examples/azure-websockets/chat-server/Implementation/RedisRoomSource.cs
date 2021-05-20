using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using StackExchange.Redis;

namespace Lemvik.Example.Chat.Server.Examples.Azure.Implementation
{
    public class RedisRoomSource : IRoomSource
    {
        private readonly IDatabaseAsync database;
        private readonly IMessageTrackerFactory messageTrackerFactory;
        private readonly IRoomBackplaneFactory roomBackplaneFactory;
        private readonly TimeSpan presenceThreshold;
        private readonly int roomSize;
        private readonly string roomsKey;
        private readonly ConcurrentDictionary<string, IRoom> rooms;

        public RedisRoomSource(IDatabaseAsync database,
                               IMessageTrackerFactory messageTrackerFactory,
                               IRoomBackplaneFactory roomBackplaneFactory,
                               TimeSpan presenceThreshold,
                               int roomSize, 
                               string roomsKey = "roomsList")
        {
            this.database = database;
            this.messageTrackerFactory = messageTrackerFactory;
            this.roomBackplaneFactory = roomBackplaneFactory;
            this.presenceThreshold = presenceThreshold;
            this.roomSize = roomSize;
            this.roomsKey = roomsKey;
            this.rooms = new ConcurrentDictionary<string, IRoom>();
        }

        public Task InitializeAsync(CancellationToken token = default)
        {
            return FetchRooms(token);
        }

        public Task<IRoom> BuildRoom(string roomName, CancellationToken token = default)
        {
            var chatRoom = new ChatRoom(Guid.NewGuid().ToString(), roomName);
            return BuildRoom(chatRoom, token);
        }

        public async Task<IReadOnlyCollection<IRoom>> ExistingRooms(CancellationToken token = default)
        {
            await FetchRooms(token);
            return rooms.Values.ToList();
        }

        private async Task FetchRooms(CancellationToken token = default)
        {
            var redisRooms = await database.ListRangeAsync(roomsKey);
            var parsedRooms = redisRooms.Select(room => JsonSerializer.Deserialize<RedisRoomRec>(room))
                                        .Where(room => room != null)
                                        .Select(room => new ChatRoom(room.Id, room.Name))
                                        .Select(room => BuildRoom(room, token));

            await Task.WhenAll(parsedRooms);
        }

        private async Task<IRoom> BuildRoom(ChatRoom chatRoom, CancellationToken token = default)
        {
            if (rooms.TryGetValue(chatRoom.Id, out var existing))
            {
                return existing;
            }

            var tracker = await messageTrackerFactory.Create(chatRoom, token);
            var backplane = await roomBackplaneFactory.CreateForRoom(chatRoom, token);
            var room = new RedisRoom(chatRoom, tracker, backplane, database, presenceThreshold, roomSize);
            rooms.TryAdd(chatRoom.Id, room);
            await database.ListLeftPushAsync(roomsKey, JsonSerializer.Serialize(new RedisRoomRec
            {
                Id = chatRoom.Id,
                Name = chatRoom.Name
            }));
            return room;
        }

        private class RedisRoomRec
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
    }
}
