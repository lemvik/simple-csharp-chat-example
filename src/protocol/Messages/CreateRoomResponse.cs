namespace Critical.Chat.Protocol.Messages
{
    public class CreateRoomResponse : IResponse
    {
        public ulong RequestId { get; }
        public MessageType Type => MessageType.CreateRoomResponse;
        public IChatRoom Room { get; }

        public CreateRoomResponse(ulong id, IChatRoom room)
        {
            RequestId = id;
            Room = room;
        }
    }
}
