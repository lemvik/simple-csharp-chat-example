namespace Critical.Chat.Protocol.Messages
{
    public class CreateRoomRequest : IRequest
    {
        public ulong RequestId { get; set; }
        public MessageType Type => MessageType.CreateRoomRequest;
        public string RoomName { get; }

        public CreateRoomRequest(string roomName)
        {
            RequestId = default;
            RoomName = roomName;
        }

        public CreateRoomRequest(ulong id, string roomName)
        {
            RequestId = id;
            RoomName = roomName;
        }
    }
}
