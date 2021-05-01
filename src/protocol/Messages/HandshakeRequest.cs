namespace Critical.Chat.Protocol.Messages
{
    public class HandshakeRequest : IMessage
    {
        public ulong Id { get; }
        public IChatUser User { get; }
        public MessageType Type => MessageType.HandshakeRequest;
        
        public HandshakeRequest(ulong id, IChatUser user)
        {
            Id = id;
            User = user;
        }
    }
}
