using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Critical.Chat.Protocol;

namespace Critical.Chat.Server.Examples.TCP
{
    public interface IChatUserIdentityProvider
    {
        Task<IChatUser> Identify(TcpClient client, CancellationToken token = default);
    }
}
