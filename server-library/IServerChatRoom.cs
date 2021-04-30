using System.Collections.Generic;
using System.Threading.Tasks;
using Critical.Chat.Protocol;

namespace Critical.Chat.Server
{
    public interface IServerChatRoom : IChatRoom
    {
        Task<IChatRoomUser> AddUser(IChatUser user);
        
        Task<IReadOnlyCollection<IChatRoomUser>> ListUsers();
        
        Task Close();
    }
}
