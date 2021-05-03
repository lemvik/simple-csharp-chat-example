namespace Lemvik.Example.Chat.Protocol.Messages
{
    public class HandshakeRequest : IMessage
    {
        public ChatUser User { get; }
        
        public HandshakeRequest(ChatUser user)
        {
            User = user;
        }

        public override string ToString()
        {
            return $"HandshakeRequest[User={User}]";
        }
    }
}
