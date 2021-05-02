using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using Lemvik.Example.Chat.Protocol.Messages;

namespace Lemvik.Example.Chat.Server
{
    public interface IConnectedClient
    {
        ChatUser User { get; } 
        
        void EnterRoom(IServerChatRoom serverChatRoom);

        void LeaveRoom(IServerChatRoom serverChatRoom);
        
        Task RunAsync(CancellationToken token = default);
        
        Task SendMessage(IMessage message, CancellationToken token = default);
    }
}
