using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Critical.Chat.Protocol;

namespace Critical.Chat.Server.Implementation
{
    internal class ServerChatRoom : IServerChatRoom
    {
        public string Id { get; }
        public string Name { get; }

        private readonly ConcurrentDictionary<string, IConnectedClient> clients;
        private readonly Channel<IChatMessage> messages;

        internal ServerChatRoom(string id, string name)
        {
            Id = id;
            Name = name;
            this.clients = new ConcurrentDictionary<string, IConnectedClient>();
            this.messages = Channel.CreateUnbounded<IChatMessage>();
        }

        public async Task AddMessage(IChatMessage message, CancellationToken token = default)
        {
            await messages.Writer.WriteAsync(message, token);
        }

        public Task AddUser(IConnectedClient connectedClient, CancellationToken token = default)
        {
            if (!clients.TryAdd(connectedClient.User.Id, connectedClient))
            {
                throw new Exception($"Cannot add [client={connectedClient}] to chat [room={this}]");
            }
            
            return Task.CompletedTask;
        }

        public Task RemoveUser(IConnectedClient connectedClient, CancellationToken token = default)
        {
            if (!clients.TryRemove(connectedClient.User.Id, out var _))
            {
                throw new Exception($"Cannot remove [client={connectedClient}] to chat [room={this}]");
            }

            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<IChatRoomUser>> ListUsers(CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }
        
        public Task<IReadOnlyCollection<IChatMessage>> MostRecentMessages(
            int maxMessages, CancellationToken token = default)
        {
            var chatMessages = new List<IChatMessage>
            {
                new ChatMessage(new ChatUser("1", "Test"), this, "Some message")
            };
            return Task.FromResult<IReadOnlyCollection<IChatMessage>>(chatMessages);
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
