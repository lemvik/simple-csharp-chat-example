using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using Microsoft.AspNetCore.Http;

namespace Lemvik.Example.Chat.Server.Examples.Azure
{
    public interface IChatUserIdentityProvider
    {
        Task<ChatUser> Identify(HttpContext client, CancellationToken token = default);
    }
}
