using System.Collections.Generic;

namespace Critical.Chat.Protocol.Messages
{
    public class ListRoomsResponse : IResponse
    {
        public ulong RequestId { get; }
        public MessageType Type => MessageType.ListRoomsResponse;

        public IReadOnlyCollection<IChatRoom> Rooms { get; }

        public ListRoomsResponse(ulong id, IReadOnlyCollection<IChatRoom> rooms)
        {
            RequestId = id;
            Rooms = rooms;
        }
    }
}
