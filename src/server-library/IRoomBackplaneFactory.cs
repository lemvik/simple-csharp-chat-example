using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;

namespace Lemvik.Example.Chat.Server
{
    public interface IRoomBackplaneFactory
    {
        Task<IRoomBackplane> CreateForRoom(ChatRoom chatRoom, CancellationToken token = default); 
    }
}
