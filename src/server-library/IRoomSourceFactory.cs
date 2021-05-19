using System.Threading;
using System.Threading.Tasks;

namespace Lemvik.Example.Chat.Server
{
    public interface IRoomSourceFactory
    {
        Task<IRoomSource> CreateAsync(CancellationToken token);
    }
}
