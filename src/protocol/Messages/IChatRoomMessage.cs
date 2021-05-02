namespace Lemvik.Example.Chat.Protocol.Messages
{
    public interface IChatRoomMessage : IMessage
    {
        ChatRoom Room { get; } 
    }
}
