using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Critical.Chat.Protocol;
using Critical.Chat.Protocol.Messages;

namespace Critical.Chat.Server
{
    public interface IServerChatRoom : IChatRoom
    {
        ChannelWriter<(IChatRoomMessage, IConnectedClient)> MessagesSink { get; }

        Task AddUser(IConnectedClient connectedClient, CancellationToken token = default);

        Task RemoveUser(IConnectedClient connectedClient, CancellationToken token = default);

        Task<IReadOnlyCollection<IChatMessage>> MostRecentMessages(int maxMessages, CancellationToken token = default);
    }
}
