using System.Collections.Generic;

namespace Lemvik.Example.Chat.Protocol.Messages
{
    public class ListRoomsResponse : IMessage
    {
        public IReadOnlyCollection<ChatRoom> Rooms { get; }

        public ListRoomsResponse(IReadOnlyCollection<ChatRoom> rooms)
        {
            Rooms = rooms;
        }
    }
}
