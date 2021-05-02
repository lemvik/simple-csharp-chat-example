using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using Lemvik.Example.Chat.Protocol.Messages;

namespace Lemvik.Example.Chat.Server
{
    public interface IServerChatRoom : IChatRoom
    {
        ChannelWriter<(IMessage, IConnectedClient)> MessagesSink { get; }

        Task AddUser(IConnectedClient connectedClient, CancellationToken token = default);

        Task RemoveUser(IConnectedClient connectedClient, CancellationToken token = default);

        Task<IReadOnlyCollection<IChatMessage>> MostRecentMessages(int maxMessages, CancellationToken token = default);
    }
}
