using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using Lemvik.Example.Chat.Protocol.Messages;
using StackExchange.Redis;

namespace Lemvik.Example.Chat.Server.Examples.Azure.Implementation
{
    public class RedisMessageTracker : IMessageTracker
    {
        private readonly ChatRoom chatRoom;
        private readonly IDatabaseAsync database;
        private readonly string chatKey;
        private readonly int historyLength;

        public RedisMessageTracker(ChatRoom chatRoom, IDatabaseAsync database, int historyLength = 50)
        {
            this.chatRoom = chatRoom;
            this.database = database;
            this.historyLength = historyLength;
            this.chatKey = $"chatRoom:messageHistory:{chatRoom.Id}";
        }

        public async Task TrackMessage(ChatMessage chatMessage, CancellationToken token = default)
        {
            await database.ListLeftPushAsync(chatKey, RedisChatMessageSerializer.AsString(chatMessage));
            await database.ListTrimAsync(chatKey, 0, historyLength);
        }

        public async Task<IReadOnlyCollection<ChatMessage>> LastMessages(uint count, CancellationToken token = default)
        {
            var messages = await database.ListRangeAsync(chatKey, 0, count);
            return messages.Where(mes => mes.HasValue)
                           .Select(mes => RedisChatMessageSerializer.FromString(chatRoom, mes.ToString())).ToArray();
        }
    }
}
