namespace Lemvik.Example.Chat.Protocol.Messages
{
    public class LeaveRoomResponse : IMessage
    {
        public ChatRoom Room { get; }

        public LeaveRoomResponse(ChatRoom room)
        {
            Room = room;
        }

        public override string ToString()
        {
            return $"LeaveRoomResponse[Room={Room}]";
        }
    }
}
