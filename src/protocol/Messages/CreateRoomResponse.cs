namespace Critical.Chat.Protocol.Messages
{
    public class CreateRoomResponse : IMessage
    {
        public IChatRoom Room { get; }

        public CreateRoomResponse(IChatRoom room)
        {
            Room = room;
        }
    }
}
