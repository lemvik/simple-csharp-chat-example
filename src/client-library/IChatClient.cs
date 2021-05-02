using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;

namespace Lemvik.Example.Chat.Client
{
    public interface IChatClient
    {
        Task RunAsync(CancellationToken token = default);

        Task<IReadOnlyCollection<IChatRoom>> ListRooms(CancellationToken token = default);

        Task<IChatRoom> CreateRoom(string roomName, CancellationToken token = default);

        Task<IClientChatRoom> JoinRoom(IChatRoom room, CancellationToken token = default);
    }
}
