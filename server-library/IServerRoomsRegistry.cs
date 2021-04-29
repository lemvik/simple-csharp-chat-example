using System.Collections.Generic;
using System.Threading.Tasks;
using Critical.Chat.Protocol;

namespace Critical.Chat.Server
{
    public interface IServerRoomsRegistry
    {
        Task<IServerChatRoom> CreateRoom(string roomName);
        Task<IReadOnlyCollection<IServerChatRoom>> ListRooms();

        Task CloseRoom(IChatRoom room);
    }
}
