using System.Collections.Generic;

namespace Critical.Chat.Protocol.Messages
{
    public class ListRoomsResponse : IMessage
    {
        public ulong Id { get; }
        public MessageType Type => MessageType.ListRoomsResponse;

        public IReadOnlyCollection<IChatRoom> Rooms { get; }

        public ListRoomsResponse(ulong id, IReadOnlyCollection<IChatRoom> rooms)
        {
            Id = id;
            Rooms = rooms;
        }
    }
}
