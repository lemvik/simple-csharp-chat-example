using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;

namespace Lemvik.Example.Chat.Server.Examples.TCP
{
    public class RandomChatUserIdentityProvider : IChatUserIdentityProvider
    {
        public Task<ChatUser> Identify(TcpClient client, CancellationToken token = default)
        {
            return Task.FromResult(new ChatUser(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()));
        }
    }
}
