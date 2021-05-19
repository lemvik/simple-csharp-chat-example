using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using Lemvik.Example.Chat.Protocol.Messages;
using StackExchange.Redis;

namespace Lemvik.Example.Chat.Server.Examples.Azure.Implementation
{
    public class RedisRoomBackplane : IRoomBackplane
    {
        private readonly ChatRoom chatRoom;
        private readonly string roomKey;
        private readonly ISubscriber subscriber;
        private readonly ChannelMessageQueue messageQueue;

        public RedisRoomBackplane(ChatRoom chatRoom, ISubscriber subscriber)
        {
            this.subscriber = subscriber;
            this.chatRoom = chatRoom;
            this.roomKey = $"chatRoom:messages:{chatRoom.Id}";
            this.messageQueue = subscriber.Subscribe(roomKey);
        }

        public Task AddMessage(ChatMessage message, CancellationToken token = default)
        {
            return subscriber.PublishAsync(roomKey, RedisChatMessageSerializer.AsString(message));
        }

        public async Task<ChatMessage> ReceiveMessage(CancellationToken token = default)
        {
            var incoming = await messageQueue.ReadAsync(token);
            return RedisChatMessageSerializer.FromString(chatRoom, incoming.Message);
        }
    }
}
