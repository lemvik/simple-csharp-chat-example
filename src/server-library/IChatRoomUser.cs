using Lemvik.Example.Chat.Protocol;

namespace Lemvik.Example.Chat.Server
{
    public interface IChatRoomUser : IChatUser
    {
        IChatRoom Room { get; }
        IConnectedClient Client { get; }
    }
}
