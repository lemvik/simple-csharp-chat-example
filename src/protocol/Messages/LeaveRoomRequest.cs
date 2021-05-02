namespace Lemvik.Example.Chat.Protocol.Messages
{
    public class LeaveRoomRequest : IMessage
    {
        public IChatRoom Room { get; }

        public LeaveRoomRequest(IChatRoom room)
        {
            Room = room;
        }
    }
}
