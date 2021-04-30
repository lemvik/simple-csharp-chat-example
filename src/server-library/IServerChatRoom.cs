using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Critical.Chat.Protocol;

namespace Critical.Chat.Server
{
    public interface IServerChatRoom : IChatRoom
    {
        Task AddMessage(IChatMessage message, CancellationToken token = default);
        
        Task AddUser(IConnectedClient connectedClient, CancellationToken token = default);

        Task RemoveUser(IConnectedClient connectedClient, CancellationToken token = default);

        Task<IReadOnlyCollection<IChatRoomUser>> ListUsers(CancellationToken token = default);

        Task<IReadOnlyCollection<IChatMessage>> MostRecentMessages(int maxMessages, CancellationToken token = default);

        Task Close();
    }
}
