namespace Lemvik.Example.Chat.Protocol.Messages
{
    public interface IChatRoomMessage : IMessage
    {
        IChatRoom Room { get; } 
    }
}
