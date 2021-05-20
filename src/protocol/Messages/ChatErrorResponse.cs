namespace Lemvik.Example.Chat.Protocol.Messages
{
    public class ChatErrorResponse : IMessage
    {
        public string Description { get; }

        public ChatErrorResponse(string description)
        {
            Description = description;
        }
    }
}
