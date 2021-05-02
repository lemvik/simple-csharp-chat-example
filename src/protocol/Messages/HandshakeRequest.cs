namespace Lemvik.Example.Chat.Protocol.Messages
{
    public class HandshakeRequest : IMessage
    {
        public IChatUser User { get; }
        
        public HandshakeRequest(IChatUser user)
        {
            User = user;
        }
    }
}
