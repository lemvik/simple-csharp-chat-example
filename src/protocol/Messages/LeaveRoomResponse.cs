namespace Critical.Chat.Protocol.Messages
{
    public class LeaveRoomResponse : IMessage
    {
        public IChatRoom Room { get; }

        public LeaveRoomResponse(IChatRoom room)
        {
            Room = room;
        }
    }
}
