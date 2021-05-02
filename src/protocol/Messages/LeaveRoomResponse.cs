namespace Lemvik.Example.Chat.Protocol.Messages
{
    public class LeaveRoomResponse : IMessage
    {
        public ChatRoom Room { get; }

        public LeaveRoomResponse(ChatRoom room)
        {
            Room = room;
        }
    }
}
