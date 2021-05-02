using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;

namespace Lemvik.Example.Chat.Client
{
    public interface IChatClient
    {
        Task RunAsync(CancellationToken token = default);

        Task<IReadOnlyCollection<ChatRoom>> ListRooms(CancellationToken token = default);

        Task<ChatRoom> CreateRoom(string roomName, CancellationToken token = default);

        Task<IClientChatRoom> JoinRoom(ChatRoom room, CancellationToken token = default);
    }
}
