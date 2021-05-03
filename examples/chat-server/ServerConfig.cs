using Lemvik.Example.Chat.Protocol;

namespace Lemvik.Example.Chat.Server.Examples.TCP
{
    public class ServerConfig
    {
        public class ListeningConfig
        {
            public string Host { get; set; }
            public int Port { get; set; } 
        }

        public class ChatRoomConfig 
        {
            public string Id { get; set; }
            public string Name { get; set; }

            public ChatRoom ToRoom()
            {
                return new ChatRoom(Id, Name);
            }
        }
        
        public ListeningConfig Listening { get; set; } 
        public ChatRoomConfig[] PredefinedRooms { get; set; }
    }
}
