using System;
using System.Threading.Tasks;

namespace Critical.Chat.Protocol
{
    public interface IChatRoomUser
    {
        IChatUser User { get; } 
        IChatRoom Room { get; }
        
        event Action<IChatMessage> MessageSent;
        Task ReceiveMessage(IChatMessage chatMessage);
        Task LeaveRoom(LeaveReason reason);
    }
}
