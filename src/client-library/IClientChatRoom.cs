using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;

namespace Lemvik.Example.Chat.Client
{
    public interface IClientChatRoom : IChatRoom
    {
        bool IsActive { get; }

        Task<IReadOnlyCollection<IChatUser>> ListUsers(CancellationToken token = default);
        
        Task SendMessage(string message, CancellationToken token = default);

        Task<IChatMessage> GetMessage(CancellationToken token = default);
    }
}
