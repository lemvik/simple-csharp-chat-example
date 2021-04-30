namespace Critical.Chat.Protocol.Messages
{
    public class CreateRoomResponse : IMessage
    {
        public ulong Id { get; }
        public MessageType Type => MessageType.CreateRoomResponse;
        public IChatRoom Room { get; }

        public CreateRoomResponse(ulong id, IChatRoom room)
        {
            Id = id;
            Room = room;
        }
    }
}
