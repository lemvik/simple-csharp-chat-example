namespace Critical.Chat.Protocol.Messages
{
    public class HandshakeRequest : IMessage
    {
        public ulong Id { get; }
        public string UserId { get; }
        public MessageType Type => MessageType.HandshakeRequest;
        
        public HandshakeRequest(ulong id, string userId)
        {
            Id = id;
            UserId = userId;
        }
    }
}
