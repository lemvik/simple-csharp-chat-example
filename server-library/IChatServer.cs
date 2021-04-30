using System.Threading;
using System.Threading.Tasks;
using Critical.Chat.Protocol.Transport;

namespace Critical.Chat.Server
{
    public interface IChatServer
    {
        Task RunAsync(CancellationToken token = default);
        
        Task AddClientAsync(IChatTransport transport, CancellationToken token = default);
    }
}
