using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using Lemvik.Example.Chat.Protocol.Messages;

namespace Lemvik.Example.Chat.Server
{
    public interface IServerChatRoom 
    {
        ChatRoom Room { get; } 
        
        ChannelWriter<(IMessage, IConnectedClient)> MessagesSink { get; }

        Task AddUser(IConnectedClient connectedClient, CancellationToken token = default);

        Task RemoveUser(IConnectedClient connectedClient, CancellationToken token = default);

        Task<IReadOnlyCollection<ChatMessage>> MostRecentMessages(uint maxMessages, CancellationToken token = default);
    }
}
