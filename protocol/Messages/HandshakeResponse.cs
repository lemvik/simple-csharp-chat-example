namespace Critical.Chat.Protocol.Messages
{
    public class HandshakeResponse : IMessage
    {
        public ulong Id { get; }
        public string UserName { get; }
        public MessageType Type => MessageType.HandshakeResponse;

        public HandshakeResponse(ulong id, string userName)
        {
            Id = id;
            UserName = userName;
        }
    }
}
