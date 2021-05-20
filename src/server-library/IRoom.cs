using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using Lemvik.Example.Chat.Protocol.Messages;
using Lemvik.Example.Chat.Shared;

namespace Lemvik.Example.Chat.Server
{
    public interface IRoom : IAsyncRunnable
    {
        ChatRoom ChatRoom { get; }

        Task AddMessage(IMessage message, IClient client, CancellationToken token = default);

        Task<IReadOnlyCollection<ChatMessage>> MostRecentMessages(uint maxMessages, CancellationToken token = default);

        Task<bool> AddUser(IClient client, CancellationToken token = default);

        Task RemoveUser(IClient client, CancellationToken token = default);
    }
}
