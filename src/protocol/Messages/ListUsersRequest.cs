namespace Lemvik.Example.Chat.Protocol.Messages
{
    public class ListUsersRequest : IChatRoomMessage
    {
        public ChatRoom Room { get; }
        
        public ListUsersRequest(ChatRoom room)
        {
            Room = room;
        }

        public override string ToString()
        {
            return $"ListUsersRequest[Room={Room}]";
        }
    }
}
