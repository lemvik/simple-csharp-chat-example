namespace Lemvik.Example.Chat.Protocol.Messages
{
    public class LeaveRoomRequest : IMessage
    {
        public ChatRoom Room { get; }

        public LeaveRoomRequest(ChatRoom room)
        {
            Room = room;
        }
    }
}
