namespace Critical.Chat.Protocol
{
    public class ChatMessage : IChatMessage
    {
        public IChatUser Sender { get; }
        public IChatRoom Room { get; }
        public string Body { get; }
        
        public ChatMessage(IChatUser sender, IChatRoom room, string body)
        {
            Sender = sender;
            Room = room;
            Body = body;
        }
    }
}
