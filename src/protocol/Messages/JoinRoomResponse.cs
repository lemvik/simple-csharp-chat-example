using System.Collections.Generic;

namespace Critical.Chat.Protocol.Messages
{
    public class JoinRoomResponse : IResponse
    {
        public ulong RequestId { get; }
        public MessageType Type => MessageType.JoinRoomResponse;
        public IChatRoom Room { get; }
        public IReadOnlyCollection<IChatMessage> Messages { get; }
        
        public JoinRoomResponse(ulong id, IChatRoom room, IReadOnlyCollection<IChatMessage> messages)
        {
            RequestId = id;
            Room = room;
            Messages = messages;
        }
    }
}
