using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol.Messages;

namespace Lemvik.Example.Chat.Server
{
    public interface IRoomBackplane
    {
        Task AddMessage(ChatMessage message, CancellationToken token = default);
        Task<ChatMessage> ReceiveMessage(CancellationToken token = default);
    }
}
