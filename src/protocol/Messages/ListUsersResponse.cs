using System.Collections.Generic;

namespace Lemvik.Example.Chat.Protocol.Messages
{
    public class ListUsersResponse : IChatRoomMessage
    {
        public ChatRoom Room { get; }
        public IReadOnlyCollection<ChatUser> Users { get; }
        
        public ListUsersResponse(ChatRoom room, IReadOnlyCollection<ChatUser> users)
        {
            Room = room;
            Users = users;
        }

        public override string ToString()
        {
            return $"ListUsersResponse[Room={Room},Users={string.Join(",", Users)}]";
        }
    }
}
