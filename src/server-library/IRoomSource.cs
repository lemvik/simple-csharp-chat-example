using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lemvik.Example.Chat.Server
{
    public interface IRoomSource
    {
        Task InitializeAsync(CancellationToken token = default); 
        
        Task<IRoom> BuildRoom(string roomName, CancellationToken token = default);

        Task<IReadOnlyCollection<IRoom>> ExistingRooms(CancellationToken token = default);
    }
}
