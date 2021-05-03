using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using Lemvik.Example.Chat.Protocol.Messages;

namespace Lemvik.Example.Chat.Server
{
    public interface IClient
    {
        ChatUser User { get; } 
        
        void EnterRoom(IRoom room);

        void LeaveRoom(IRoom room);
        
        Task RunAsync(CancellationToken token = default);
        
        Task SendMessage(IMessage message, CancellationToken token = default);
    }
}
