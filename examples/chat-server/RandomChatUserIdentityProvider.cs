using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;

namespace Lemvik.Example.Chat.Server.Examples.TCP
{
    public class RandomChatUserIdentityProvider : IChatUserIdentityProvider
    {
        private class ChatUser : IChatUser
        {
            public string Id { get; }
            public string Name { get; }
            
            public ChatUser(string id, string name)
            {
                Id = id;
                Name = name;
            }
        } 
        
        public Task<IChatUser> Identify(TcpClient client, CancellationToken token = default)
        {
            return Task.FromResult<IChatUser>(new ChatUser(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()));
        }
    }
}
