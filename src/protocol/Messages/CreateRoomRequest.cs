namespace Lemvik.Example.Chat.Protocol.Messages
{
    public class CreateRoomRequest : IMessage
    {
        public string RoomName { get; }

        public CreateRoomRequest(string roomName)
        {
            RoomName = roomName;
        }
    }
}
