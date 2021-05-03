using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol.Messages;

namespace Lemvik.Example.Chat.Protocol.Transport
{
    public interface IChatTransport
    {
        Task Send(IMessage message, CancellationToken token = default);

        Task<IMessage> Receive(CancellationToken token = default);

        void Close();
    }
}
