using System.Threading;
using System.Threading.Tasks;
using Critical.Chat.Protocol.Messages;

namespace Critical.Chat.Protocol.Transport
{
    public interface IChatRequestTransport : IChatTransport
    {
        Task<TResponse> Exchange<TResponse>(IRequest request, CancellationToken token = default)
            where TResponse : IMessage;
    }
}
