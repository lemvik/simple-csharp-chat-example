namespace Critical.Chat.Protocol.Messages
{
    public class LeaveRoomRequest : IMessage
    {
        public ulong Id { get; }
        public MessageType Type => MessageType.LeaveRoomRequest;
        public IChatRoom Room { get; }
        
        public LeaveRoomRequest(ulong id, IChatRoom room)
        {
            Id = id;
            Room = room;
        }
    }
}
