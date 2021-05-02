using Critical.Chat.Protocol;

namespace Critical.Chat.Server
{
    public interface IChatRoomUser : IChatUser
    {
        IChatRoom Room { get; }
        IConnectedClient Client { get; }
    }
}
