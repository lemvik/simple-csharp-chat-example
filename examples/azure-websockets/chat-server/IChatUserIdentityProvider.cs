using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;

namespace Lemvik.Example.Chat.Server.Examples.Azure
{
    public interface IChatUserIdentityProvider
    {
        Task<ChatUser> Identify(TcpClient client, CancellationToken token = default);
    }
}
