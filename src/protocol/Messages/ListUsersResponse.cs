using System.Collections.Generic;

namespace Critical.Chat.Protocol.Messages
{
    public class ListUsersResponse : IChatRoomMessage, IResponse
    {
        public ulong RequestId { get; }
        public IChatRoom Room { get; }
        public MessageType Type => MessageType.ListUsersResponse;
        public IReadOnlyCollection<IChatUser> Users { get; }
        
        public ListUsersResponse(ulong id, IChatRoom room, IReadOnlyCollection<IChatUser> users)
        {
            RequestId = id;
            Room = room;
            Users = users;
        }
    }
}
