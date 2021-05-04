using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using Lemvik.Example.Chat.Protocol.Messages;
using Lemvik.Example.Chat.Shared;

namespace Lemvik.Example.Chat.Server
{
    public interface IClient : IAsyncRunnable
    {
        ChatUser User { get; } 
        
        Task SendMessage(IMessage message, CancellationToken token = default);
    }
}
