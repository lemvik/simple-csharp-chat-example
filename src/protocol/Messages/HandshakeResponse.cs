namespace Critical.Chat.Protocol.Messages
{
    public class HandshakeResponse : IMessage
    {
        public ulong Id { get; }
        public MessageType Type => MessageType.HandshakeResponse;

        public HandshakeResponse(ulong id)
        {
            Id = id;
        }
    }
}
