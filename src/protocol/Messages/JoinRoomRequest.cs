namespace Critical.Chat.Protocol.Messages
{
    public class JoinRoomRequest : IRequest
    {
        public ulong RequestId { get; set; }
        public MessageType Type => MessageType.JoinRoomRequest;
        public string RoomId { get; }

        public JoinRoomRequest(string roomId)
        {
            RequestId = default;
            RoomId = roomId;
        }
        
        public JoinRoomRequest(ulong requestId, string roomId)
        {
            RequestId = requestId;
            RoomId = roomId;
        }
    }
}
