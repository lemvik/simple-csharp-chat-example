using System.Threading;
using System.Threading.Tasks;

namespace Lemvik.Example.Chat.Shared
{
    public interface IAsyncRunnable
    {
        Task RunAsync(CancellationToken token = default);
    }
}
