namespace Critical.Chat.Protocol.Messages
{
    public class LeaveRoomRequest : IRequest
    {
        public ulong RequestId { get; set; }
        public MessageType Type => MessageType.LeaveRoomRequest;
        public IChatRoom Room { get; }
        
        public LeaveRoomRequest(ulong id, IChatRoom room)
        {
            RequestId = id;
            Room = room;
        }
    }
}
