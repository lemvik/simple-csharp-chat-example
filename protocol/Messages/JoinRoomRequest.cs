namespace Critical.Chat.Protocol.Messages
{
    public class JoinRoomRequest : IMessage
    {
        public ulong Id { get; }
        public MessageType Type => MessageType.JoinRoomRequest;
        public string RoomId { get; }

        public JoinRoomRequest(ulong id, string roomId)
        {
            Id = id;
            RoomId = roomId;
        }
    }
}
