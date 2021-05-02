using System.Collections.Generic;

namespace Critical.Chat.Protocol.Messages
{
    public class ListUsersResponse : IChatRoomMessage
    {
        public IChatRoom Room { get; }
        public IReadOnlyCollection<IChatUser> Users { get; }
        
        public ListUsersResponse(IChatRoom room, IReadOnlyCollection<IChatUser> users)
        {
            Room = room;
            Users = users;
        }
    }
}
