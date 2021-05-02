namespace Lemvik.Example.Chat.Protocol.Messages
{
    public class ListUsersRequest : IChatRoomMessage
    {
        public IChatRoom Room { get; }
        
        public ListUsersRequest(IChatRoom room)
        {
            Room = room;
        }
    }
}
