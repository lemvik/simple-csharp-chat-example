using System.Collections.Generic;

namespace Critical.Chat.Protocol.Messages
{
    public class JoinRoomResponse : IMessage
    {
        public ulong Id { get; }
        public MessageType Type => MessageType.JoinRoomResponse;
        public IChatRoom Room { get; }
        public IReadOnlyCollection<IChatMessage> Messages { get; }
        
        public JoinRoomResponse(ulong id, IChatRoom room, IReadOnlyCollection<IChatMessage> messages)
        {
            Id = id;
            Room = room;
            Messages = messages;
        }
    }
}
