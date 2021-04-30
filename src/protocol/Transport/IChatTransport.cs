using System;
using System.Threading;
using System.Threading.Tasks;
using Critical.Chat.Protocol.Messages;

namespace Critical.Chat.Protocol.Transport
{
    public interface IChatTransport
    {
        Task Send(IMessage message, CancellationToken token = default);

        Task<IMessage> Receive(CancellationToken token = default);
    }
}
