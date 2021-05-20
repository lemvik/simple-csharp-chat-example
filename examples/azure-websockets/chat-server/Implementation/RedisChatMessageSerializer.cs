using System.Text.Json;
using Lemvik.Example.Chat.Protocol;
using Lemvik.Example.Chat.Protocol.Messages;

namespace Lemvik.Example.Chat.Server.Examples.Azure.Implementation
{
    internal static class RedisChatMessageSerializer
    {
        public static ChatMessage FromString(ChatRoom room, string incoming)
        {
            var message = JsonSerializer.Deserialize<RedisChatMessage>(incoming);
            if (message != null)
            {
                return new ChatMessage(new ChatUser(message.UserId, message.UserName),
                                       room,
                                       message.Message);
            }

            return null;
        }

        public static string AsString(ChatMessage message)
        {
            var redisMessage = new RedisChatMessage
            {
                UserId = message.Sender.Id,
                UserName = message.Sender.Name,
                Message = message.Body
            };
            return JsonSerializer.Serialize(redisMessage);
        }

        private class RedisChatMessage
        {
            public string UserId { get; set; }
            public string UserName { get; set; }
            public string Message { get; set; }
        }
    }
}
