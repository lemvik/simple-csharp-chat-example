namespace Lemvik.Example.Chat.Protocol.Messages
{
    public class CreateRoomResponse : IMessage
    {
        public ChatRoom Room { get; }

        public CreateRoomResponse(ChatRoom room)
        {
            Room = room;
        }

        public override string ToString()
        {
            return $"CreateRoomResponse[Room={Room}]";
        }
    }
}
