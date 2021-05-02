using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Critical.Chat.Protocol;

namespace Critical.Chat.Server
{
    public interface IRoomRegistry
    {
        Task<IServerChatRoom> CreateRoom(string roomName, CancellationToken roomLifetimeToken = default);

        Task<IServerChatRoom> GetRoom(string roomId, CancellationToken token = default);

        Task<IReadOnlyCollection<IServerChatRoom>> ListRooms();

        Task CloseRoom(IChatRoom room);

        Task Close();
    }
}
