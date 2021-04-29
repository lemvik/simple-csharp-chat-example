using System;
using System.Threading.Tasks;
using Critical.Chat.Protocol;

namespace Critical.Chat.Server
{
    public interface IChatRoomUser
    {
        IChatUser User { get; } 
        IServerChatRoom Room { get; }
        
        event Action<IChatMessage> MessageSent;
        Task ReceiveMessage(IChatMessage chatMessage);
        Task LeaveRoom(LeaveReason reason);
    }
}
