using System.Collections.Generic;
using System.Linq;

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

        public override string ToString()
        {
            return $"JoinRoomResponse[Room={Room},Messages={string.Join(",", Messages)}]";
        }
    }
}
