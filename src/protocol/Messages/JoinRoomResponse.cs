using System.Collections.Generic;

namespace Lemvik.Example.Chat.Protocol.Messages
{
    public class JoinRoomResponse : IMessage
    {
        public IChatRoom Room { get; }
        public IReadOnlyCollection<IChatMessage> Messages { get; }

        public JoinRoomResponse(IChatRoom room, IReadOnlyCollection<IChatMessage> messages)
        {
            Room = room;
            Messages = messages;
        }
    }
}
