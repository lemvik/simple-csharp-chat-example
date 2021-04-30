namespace Critical.Chat.Protocol.Messages
{
    public class CreateRoomRequest : IMessage
    {
        public ulong Id { get; }
        public MessageType Type => MessageType.CreateRoomRequest;
        public string RoomName { get; }
        
        public CreateRoomRequest(ulong id, string roomName)
        {
            Id = id;
            RoomName = roomName;
        }
    }
}
