using System.Collections.Generic;
using System.Threading.Tasks;
using Critical.Chat.Protocol;

namespace Critical.Chat.Server.Implementation
{
    internal class ServerChatRoom : IServerChatRoom
    {
        public string Id { get; }
        public string Name { get; }
        
        internal ServerChatRoom(string id, string name)
        {
            Id = id;
            Name = name;
        }
        
        public Task<IChatRoomUser> AddUser(IChatUser user)
        {
            throw new System.NotImplementedException();
        }

        public Task<IReadOnlyCollection<IChatRoomUser>> ListUsers()
        {
            throw new System.NotImplementedException();
        }

        public Task Close()
        {
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            return $"Room[id={Id},name={Name}]";
        }
    }
}
