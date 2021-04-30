namespace Critical.Chat.Protocol.Messages
{
    public class ListRoomsRequest : IMessage
    {
        public ulong Id { get; }
        public MessageType Type => MessageType.ListRoomsRequest;

        public ListRoomsRequest(ulong id)
        {
            Id = id;
        }
    }
}
