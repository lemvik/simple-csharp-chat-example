namespace Critical.Chat.Protocol.Messages
{
    public class ListRoomsRequest : IRequest
    {
        public ulong RequestId { get; set; }
        public MessageType Type => MessageType.ListRoomsRequest;

        public ListRoomsRequest(ulong id = default)
        {
            RequestId = id;
        }
    }
}
