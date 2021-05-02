using Critical.Chat.Protocol.Messages;

namespace Critical.Chat.Protocol
{
    public class ChatMessage : IChatMessage, IChatRoomMessage
    {
        public IChatUser Sender { get; }
        public IChatRoom Room { get; }
        public string Body { get; }
        public MessageType Type => MessageType.ChatMessage;

        public ChatMessage(IChatUser sender, IChatRoom room, string body)
        {
            Sender = sender;
            Room = room;
            Body = body;
        }
    }
}
