namespace Lemvik.Example.Chat.Protocol.Messages
{
    public class JoinRoomRequest : IMessage
    {
        public string RoomId { get; }

        public JoinRoomRequest(string roomId)
        {
            RoomId = roomId;
        }

        public override string ToString()
        {
            return $"JoinRoomRequest[RoomId={RoomId}]";
        }
    }
}
