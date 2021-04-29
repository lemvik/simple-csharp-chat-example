using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Critical.Chat.Protocol;
using Microsoft.Extensions.Logging;

namespace Critical.Chat.Server.Implementation
{
    public class ServerRoomsRegistry : IServerRoomsRegistry
    {
        private readonly ILogger<ServerRoomsRegistry> logger;
        private readonly List<ServerChatRoom> rooms = new List<ServerChatRoom>();

        public ServerRoomsRegistry(ILogger<ServerRoomsRegistry> logger)
        {
            this.logger = logger;
        }
        
        public Task<IServerChatRoom> CreateRoom(string roomName)
        {
            var existing = rooms.Find(room => room.Name.Equals(roomName));
            if (existing != null)
            {
                return Task.FromResult<IServerChatRoom>(existing);
            }

            var newRoom = new ServerChatRoom(roomName, roomName);
            rooms.Add(newRoom);
            
            logger.LogDebug("Created [room={room}]", newRoom);
            
            return Task.FromResult<IServerChatRoom>(newRoom);
        }

        public Task<IReadOnlyCollection<IServerChatRoom>> ListRooms()
        {
            return Task.FromResult<IReadOnlyCollection<IServerChatRoom>>(rooms);
        }

        public Task CloseRoom(IChatRoom room)
        {
            var ownInstance = rooms.Find(ownRoom => ownRoom.Id == room.Id);
            if (ownInstance == null)
            {
                throw new Exception($"Attempt to close non-owned [room={room}]");
            }

            rooms.Remove(ownInstance);
            
            logger.LogDebug("Closed [room={room}]", ownInstance);

            return ownInstance.Close();
        }
    }
}
