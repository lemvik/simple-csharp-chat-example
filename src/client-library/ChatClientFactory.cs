using Lemvik.Example.Chat.Protocol.Transport;
using Microsoft.Extensions.Logging;

namespace Lemvik.Example.Chat.Client
{
    public class ChatClientFactory : IChatClientFactory
    {
        private readonly ILoggerFactory loggerFactory;

        public ChatClientFactory(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        public IChatClient CreateClient(IChatTransport clientTransport, IChatClientConfiguration configuration)
        {
            return new ChatClient(loggerFactory.CreateLogger<ChatClient>(), clientTransport);
        }
    }
}