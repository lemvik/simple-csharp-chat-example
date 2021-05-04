using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using Lemvik.Example.Chat.Shared;

namespace Lemvik.Example.Chat.Client
{
    public interface IChatClient : IAsyncRunnable
    {
        ChatUser User { get; }
        
        Task<IReadOnlyCollection<ChatRoom>> ListRooms(CancellationToken token = default);

        Task<ChatRoom> CreateRoom(string roomName, CancellationToken token = default);

        Task<IRoom> JoinRoom(ChatRoom room, CancellationToken token = default);
    }
}
