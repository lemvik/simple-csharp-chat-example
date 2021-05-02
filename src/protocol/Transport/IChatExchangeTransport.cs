using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol.Messages;

namespace Lemvik.Example.Chat.Protocol.Transport
{
    public interface IChatExchangeTransport : IChatTransport
    {
        Task<TResponse> Exchange<TResponse>(IMessage request, CancellationToken token = default)
            where TResponse : IMessage;
    }
}
