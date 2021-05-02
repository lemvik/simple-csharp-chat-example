using Lemvik.Example.Chat.Protocol;

namespace Lemvik.Example.Chat.Client
{
    public interface IChatRoomUser
    {
        IChatUser User { get; } 
        IChatRoom Room { get; }
    }
}
