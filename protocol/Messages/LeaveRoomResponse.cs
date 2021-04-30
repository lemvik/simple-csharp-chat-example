namespace Critical.Chat.Protocol.Messages
{
    public class LeaveRoomResponse : IMessage
    {
        public ulong Id { get; }
        public MessageType Type => MessageType.LeaveRoomResponse;
        public IChatRoom Room { get; }

        public LeaveRoomResponse(ulong id, IChatRoom room)
        {
            Id = id;
            Room = room;
        }
    }
}
