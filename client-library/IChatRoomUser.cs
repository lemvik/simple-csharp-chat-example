using Critical.Chat.Protocol;

namespace Critical.Chat.Client
{
    public interface IChatRoomUser
    {
        IChatUser User { get; } 
        IChatRoom Room { get; }
    }
}
