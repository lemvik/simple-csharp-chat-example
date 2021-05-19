using JetBrains.Annotations;
using Lemvik.Example.Chat.Protocol;

namespace Lemvik.Example.Chat.Server.Examples.Azure
{
    public class ServerConfig
    {
        [UsedImplicitly]
        public class ListeningConfig
        {
            public string Host { get; [UsedImplicitly] set; }
            public int Port { get; [UsedImplicitly] set; }
        }

        [UsedImplicitly]
        public class ChatRoomConfig
        {
            public string Id { get; [UsedImplicitly] set; }
            public string Name { get; [UsedImplicitly] set; }

            public ChatRoom ToRoom()
            {
                return new(Id, Name);
            }
        }

        [UsedImplicitly]
        public class RedisConfig
        {
            public string Uri { get; set; }
        }

        public ListeningConfig Listening { get; [UsedImplicitly] set; }
        public ChatRoomConfig[] PredefinedRooms { get; [UsedImplicitly] set; }
        public RedisConfig Redis { get; [UsedImplicitly] set; }
    }
}
