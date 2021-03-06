using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using Lemvik.Example.Chat.Protocol.Transport;

namespace Lemvik.Example.Chat.Server
{
    public interface IChatServer
    {
        Task RunAsync(CancellationToken token = default);
        
        Task<Task> AddClientAsync(ChatUser chatUser, IChatTransport transport, CancellationToken token = default);
    }
}
