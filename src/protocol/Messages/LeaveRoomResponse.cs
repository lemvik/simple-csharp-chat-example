namespace Critical.Chat.Protocol.Messages
{
    public class LeaveRoomResponse : IResponse
    {
        public ulong RequestId { get; }
        public MessageType Type => MessageType.LeaveRoomResponse;
        public IChatRoom Room { get; }

        public LeaveRoomResponse(ulong id, IChatRoom room)
        {
            RequestId = id;
            Room = room;
        }
    }
}
