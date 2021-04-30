using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Critical.Chat.Protocol;
using Critical.Chat.Protocol.Messages;

namespace Critical.Chat.Server
{
    public interface IConnectedClient
    {
        IChatUser User { get; } 
        
        Task RunAsync(ChannelWriter<(IConnectedClient, IMessage)> sink, CancellationToken token = default);
        
        Task SendMessage(IMessage message, CancellationToken token = default);
    }
}
