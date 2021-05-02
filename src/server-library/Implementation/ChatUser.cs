using Lemvik.Example.Chat.Protocol;

namespace Lemvik.Example.Chat.Server.Implementation
{
    public class ChatUser : IChatUser
    {
        public string Id { get; }
        public string Name { get; }

        public ChatUser(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
