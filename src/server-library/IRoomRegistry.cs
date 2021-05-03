using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Shared;

namespace Lemvik.Example.Chat.Server
{
    public interface IRoomRegistry : IAsyncRunnable
    {
        Task<IRoom> CreateRoom(string roomName, CancellationToken roomLifetimeToken = default);

        Task<IRoom> GetRoom(string roomId, CancellationToken token = default);

        Task<IReadOnlyCollection<IRoom>> ListRooms();
    }
}
