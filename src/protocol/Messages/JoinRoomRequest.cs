namespace Critical.Chat.Protocol.Messages
{
    public class JoinRoomRequest : IMessage
    {
        public string RoomId { get; }

        public JoinRoomRequest(string roomId)
        {
            RoomId = roomId;
        }
    }
}
