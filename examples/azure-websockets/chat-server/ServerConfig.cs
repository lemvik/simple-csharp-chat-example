using System;
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

        [UsedImplicitly]
        public class RoomsConfig
        {
            public TimeSpan PresenceThreshold { get; [UsedImplicitly] set; } = TimeSpan.FromSeconds(5);
            public int RoomSize { get; [UsedImplicitly] set; } = 20;
            public string RoomsKey { get; [UsedImplicitly] set; }
        }

        public ListeningConfig Listening { get; [UsedImplicitly] set; }
        public ChatRoomConfig[] PredefinedRooms { get; [UsedImplicitly] set; }
        public RedisConfig Redis { get; [UsedImplicitly] set; }
        public RoomsConfig Rooms { get; [UsedImplicitly] set; }
    }
}
