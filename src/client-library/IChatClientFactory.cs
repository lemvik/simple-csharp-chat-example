using Lemvik.Example.Chat.Protocol.Transport;

namespace Lemvik.Example.Chat.Client
{
    public interface IChatClientFactory
    {
        IChatClient CreateClient(IChatTransport clientTransport, IChatClientConfiguration configuration);
    }
}
