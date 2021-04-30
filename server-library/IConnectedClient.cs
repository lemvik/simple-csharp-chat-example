using System.Threading;
using System.Threading.Tasks;
using Critical.Chat.Protocol;
using Critical.Chat.Protocol.Messages;

namespace Critical.Chat.Server
{
    public interface IConnectedClient
    {
        IChatUser User { get; } 
        
        Task RunAsync(CancellationToken token = default);
        
        Task<IMessage> ReceiveMessage(CancellationToken token = default);

        Task SendMessage(IMessage message, CancellationToken token = default);
    }
}
