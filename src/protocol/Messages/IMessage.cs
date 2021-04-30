using System;

namespace Critical.Chat.Protocol.Messages
{
    public interface IMessage
    {
        ulong Id { get;}
        MessageType Type { get; } 
    }
}
