using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol.Messages;

namespace Lemvik.Example.Chat.Server
{
    public interface IMessageTracker
    {
        Task TrackMessage(ChatMessage chatMessage, CancellationToken token = default);

        Task<IReadOnlyCollection<ChatMessage>> LastMessages(uint count, CancellationToken token = default);
    }
}
