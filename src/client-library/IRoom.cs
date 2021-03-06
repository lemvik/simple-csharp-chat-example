using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using Lemvik.Example.Chat.Protocol.Messages;

namespace Lemvik.Example.Chat.Client
{
    public interface IRoom 
    {
        ChatRoom ChatRoom { get; } 
        
        Task<IReadOnlyCollection<ChatUser>> ListUsers(CancellationToken token = default);
        
        Task SendMessage(string message, CancellationToken token = default);

        Task<ChatMessage> GetMessage(CancellationToken token = default);

        Task Leave(CancellationToken token = default);
    }
}
