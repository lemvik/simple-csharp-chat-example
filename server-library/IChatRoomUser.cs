using Critical.Chat.Protocol;

namespace Critical.Chat.Server
{
    public interface IChatRoomUser
    {
        IChatRoom Room { get; }
        IConnectedClient Client { get; }
    }
}
