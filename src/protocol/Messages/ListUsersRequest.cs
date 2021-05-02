namespace Critical.Chat.Protocol.Messages
{
    public class ListUsersRequest : IChatRoomMessage, IRequest
    {
        public ulong RequestId { get; set; }
        public IChatRoom Room { get; }
        public MessageType Type => MessageType.ListUsersRequest;
        
        public ListUsersRequest(ulong id, IChatRoom room)
        {
            RequestId = id;
            Room = room;
        }
    }
}
