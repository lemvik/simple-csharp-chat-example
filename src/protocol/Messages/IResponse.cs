namespace Critical.Chat.Protocol.Messages
{
    public interface IResponse : IMessage
    {
        ulong RequestId { get; }
    }
}
