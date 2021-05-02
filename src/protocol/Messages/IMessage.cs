using System;

namespace Critical.Chat.Protocol.Messages
{
    public interface IMessage
    {
        MessageType Type { get; } 
    }
}
