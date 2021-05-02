namespace Critical.Chat.Protocol.Messages
{
    public interface IRequest : IMessage
    {
        ulong RequestId { get; set; }
    }
}
