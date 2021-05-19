using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Lemvik.Example.Chat.Server.Examples.Azure
{
    internal interface IWebSocketAcceptor
    {
        Task AcceptWebSocket(HttpContext socketContext, WebSocket socket, CancellationToken token = default);
    }
}
