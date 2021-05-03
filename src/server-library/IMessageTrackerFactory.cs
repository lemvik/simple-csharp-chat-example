using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;

namespace Lemvik.Example.Chat.Server
{
    public interface IMessageTrackerFactory
    {
        Task<IMessageTracker> Create(ChatRoom room, CancellationToken token = default);
    }
}
