using Lemvik.Example.Chat.Protocol;

namespace Lemvik.Example.Chat.Server
{
    public interface IChatRoomUser
    {
        ChatUser User { get; }
        ChatRoom Room { get; }
        IConnectedClient Client { get; }
    }
}
