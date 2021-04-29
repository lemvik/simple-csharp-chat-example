namespace Critical.Chat.Protocol
{
    public interface IChatMessage
    {
        IChatUser Sender { get; }
        IChatRoom Room { get; }
        string Body { get; }
    }
}
