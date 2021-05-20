using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using Lemvik.Example.Chat.Server.Implementation;
using StackExchange.Redis;

namespace Lemvik.Example.Chat.Server.Examples.Azure.Implementation
{
    public class RedisRoom : Room
    {
        private readonly IDatabaseAsync database;
        private readonly TimeSpan presenceThreshold;
        private readonly int maxSize;
        private readonly string roomKey;

        public RedisRoom(ChatRoom room,
                         IMessageTracker messageTracker,
                         IRoomBackplane backplane,
                         IDatabaseAsync database,
                         TimeSpan presenceThreshold,
                         int maxSize) :
            base(room, messageTracker, backplane)
        {
            this.database = database;
            this.presenceThreshold = presenceThreshold;
            this.maxSize = maxSize;
            this.roomKey = $"room:users:{room.Id}";
        }

        public override async Task<bool> AddUser(IClient client, CancellationToken token = default)
        {
            if (!await TryAddingUserToRedisAsync(client.User, token))
            {
                return false;
            }

            if (await base.AddUser(client, token))
            {
                return true;
            }
            
            await RemoveUserFromRedisAsync(client.User, token);
            return false;
        }

        public override async Task RemoveUser(IClient client, CancellationToken token = default)
        {
            await RemoveUserFromRedisAsync(client.User, token);
            await base.RemoveUser(client, token);
        }

        protected override async Task<IEnumerable<ChatUser>> ListUsers()
        {
            var threshold = (DateTime.Now - presenceThreshold).Ticks;

            var present =
                await database.SortedSetRangeByScoreAsync(roomKey,
                                                          double.PositiveInfinity,
                                                          threshold,
                                                          Exclude.None,
                                                          Order.Descending,
                                                          0,
                                                          maxSize);

            return present.Select(rec => JsonSerializer.Deserialize<ChatUserRec>(rec))
                          .Where(rec => rec != null)
                          .Select(rec => new ChatUser(rec.Id, rec.Name));
        }

        public override async Task RunAsync(CancellationToken token = default)
        {
            await Task.WhenAll(base.RunAsync(token), UpdateTimestamps(token));
        }

        private async Task<bool> TryAddingUserToRedisAsync(ChatUser user, CancellationToken token = default)
        {
            var present = await CountRoomUsers(token);
            if (present >= maxSize)
            {
                return false;
            }
            
            var rec = new ChatUserRec {Id = user.Id, Name = user.Name};
            var serialized = JsonSerializer.Serialize(rec);
            var timestamp = DateTime.Now.Ticks;

            return await database.SortedSetAddAsync(roomKey, serialized, timestamp, When.NotExists);
        }

        private async Task<int> CountRoomUsers(CancellationToken token = default)
        {
            var threshold = (DateTime.Now - presenceThreshold).Ticks;
            return (int) await database.SortedSetLengthAsync(roomKey, threshold);
        }

        private async Task AddUserToRedisAsync(ChatUser user, CancellationToken token = default)
        {
            var rec = new ChatUserRec {Id = user.Id, Name = user.Name};
            var serialized = JsonSerializer.Serialize(rec);
            var timestamp = DateTime.Now.Ticks;

            await database.SortedSetAddAsync(roomKey, serialized, timestamp);
        }

        private async Task RemoveUserFromRedisAsync(ChatUser user, CancellationToken token = default)
        {
            var rec = new ChatUserRec {Id = user.Id, Name = user.Name};
            var serialized = JsonSerializer.Serialize(rec);

            await database.SortedSetRemoveAsync(roomKey, serialized);
        }

        private async Task UpdateTimestamps(CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(presenceThreshold / 3, token);
                foreach (var client in Clients.Values)
                {
                    await AddUserToRedisAsync(client.User, token);
                }

                await CleanStaleEntries(token);
            }
        }

        private async Task CleanStaleEntries(CancellationToken token = default)
        {
            var threshold = (DateTime.Now - presenceThreshold).Ticks;
            await database.SortedSetRemoveRangeByScoreAsync(roomKey, 0, threshold);
        }

        private class ChatUserRec
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
    }
}
