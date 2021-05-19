using System.Threading;
using System.Threading.Tasks;

namespace Lemvik.Example.Chat.Client.Examples
{
    public interface IConsoleClient
    {
        Task InteractAsync(IChatClient client, CancellationToken token = default);
    }
}
