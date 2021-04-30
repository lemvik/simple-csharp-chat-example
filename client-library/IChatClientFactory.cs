using Critical.Chat.Protocol.Transport;

namespace Critical.Chat.Client
{
    public interface IChatClientFactory
    {
        IChatClient CreateClient(IChatTransport clientTransport);
    }
}
