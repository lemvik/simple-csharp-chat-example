using System.Collections.Generic;

namespace Critical.Chat.Protocol.Messages
{
    public class ListRoomsResponse : IMessage
    {
        public IReadOnlyCollection<IChatRoom> Rooms { get; }

        public ListRoomsResponse(IReadOnlyCollection<IChatRoom> rooms)
        {
            Rooms = rooms;
        }
    }
}
