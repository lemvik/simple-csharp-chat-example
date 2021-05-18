using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using Microsoft.AspNetCore.Http;

namespace Lemvik.Example.Chat.Server.Examples.Azure
{
    public class RandomChatUserIdentityProvider : IChatUserIdentityProvider
    {
        private readonly Random generator = new();
        private readonly string[] names = {"Victor", "Alexander", "Antti", "Timo"};

        public Task<ChatUser> Identify(HttpContext client, CancellationToken token = default)
        {
            return Task.FromResult(new ChatUser(Guid.NewGuid().ToString(), names[generator.Next(names.Length)]));
        }
    }
}
