using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Critical.Chat.Protocol;
using Critical.Chat.Protocol.Messages;

namespace Critical.Chat.Server.Implementation
{
    internal class ServerChatRoom : IServerChatRoom
    {
        public string Id { get; }
        public string Name { get; }
        public ChannelWriter<(IChatRoomMessage, IConnectedClient)> MessagesSink => messages.Writer;

        private readonly ConcurrentDictionary<string, IConnectedClient> clients;
        private readonly Channel<(IChatRoomMessage, IConnectedClient)> messages;

        internal ServerChatRoom(string id, string name)
        {
            Id = id;
            Name = name;
            clients = new ConcurrentDictionary<string, IConnectedClient>();
            messages = Channel.CreateUnbounded<(IChatRoomMessage, IConnectedClient)>();
        }

        public Task AddUser(IConnectedClient connectedClient, CancellationToken token = default)
        {
            if (!clients.TryAdd(connectedClient.User.Id, connectedClient))
            {
                throw new Exception($"Cannot add [client={connectedClient}] to chat [room={this}]");
            }

            connectedClient.EnterRoom(this);

            return Task.CompletedTask;
        }

        public Task RemoveUser(IConnectedClient connectedClient, CancellationToken token = default)
        {
            if (!clients.TryRemove(connectedClient.User.Id, out var _))
            {
                throw new Exception($"Cannot remove [client={connectedClient}] to chat [room={this}]");
            }

            connectedClient.LeaveRoom(this);

            return Task.CompletedTask;
        }

        private IReadOnlyCollection<IChatRoomUser> ListUsers()
        {
            var users = clients.Values.Select(client => new ChatRoomUser(this, client)).ToList();
            return users;
        }

        public Task<IReadOnlyCollection<IChatMessage>> MostRecentMessages(
            int maxMessages, CancellationToken token = default)
        {
            return Task.FromResult<IReadOnlyCollection<IChatMessage>>(Array.Empty<IChatMessage>());
        }

        public async Task RunAsync(CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                var (message, client) = await messages.Reader.ReadAsync(token);

                await DispatchMessage(message, client, token);
            }
        }

        private async Task DispatchMessage(IChatRoomMessage message,
                                           IConnectedClient client,
                                           CancellationToken token = default)
        {
            switch (message)
            {
                case ListUsersRequest listUsersRequest:
                {
                    var users = ListUsers();
                    var response = new ListUsersResponse(listUsersRequest.RequestId, this, users);
                    await client.SendMessage(response, token);
                    break;
                }
                case ChatMessage chatMessage:
                {
                    var sendTasks = clients.Values.Select(chatClient => chatClient.SendMessage(chatMessage, token));
                    await Task.WhenAll(sendTasks);
                    break;
                }
            }
        }

        public override string ToString()
        {
            return $"Room[id={Id},name={Name}]";
        }

        private class ChatRoomUser : IChatRoomUser
        {
            public string Id => Client.User.Id;
            public string Name => Client.User.Name;
            public IChatRoom Room { get; }
            public IConnectedClient Client { get; }

            public ChatRoomUser(IChatRoom room, IConnectedClient client)
            {
                Room = room;
                Client = client;
            }
        }
    }
}
