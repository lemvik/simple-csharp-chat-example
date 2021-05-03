namespace Lemvik.Example.Chat.Protocol.Messages
{
    public class ChatMessage : IChatRoomMessage
    {
        public ChatUser Sender { get; }
        public ChatRoom Room { get; }
        public string Body { get; }

        public ChatMessage(ChatUser sender, ChatRoom room, string body)
        {
            Sender = sender;
            Room = room;
            Body = body;
        }

        public override string ToString()
        {
            return $"ChatMessage[Sender={Sender},Room={Room},Body={Body}]";
        }
    }
}
