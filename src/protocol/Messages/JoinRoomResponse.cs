using System.Collections.Generic;

namespace Lemvik.Example.Chat.Protocol.Messages
{
    public class JoinRoomResponse : IMessage
    {
        public ChatRoom Room { get; }
        public IReadOnlyCollection<ChatMessage> Messages { get; }

        public JoinRoomResponse(ChatRoom room, IReadOnlyCollection<ChatMessage> messages)
        {
            Room = room;
            Messages = messages;
        }
    }
}
