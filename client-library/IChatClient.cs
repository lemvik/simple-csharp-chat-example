using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Critical.Chat.Protocol;

namespace Critical.Chat.Client
{
    public interface IChatClient
    {
        Task RunAsync(CancellationToken token = default);
        
        Task<IReadOnlyCollection<IChatRoom>> ListRooms(CancellationToken token = default);

        Task<IChatRoomUser> JoinRoom(IChatRoom room, CancellationToken token = default);
    }
}