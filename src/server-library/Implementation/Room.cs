using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using Lemvik.Example.Chat.Protocol.Messages;
using Lemvik.Example.Chat.Shared;

namespace Lemvik.Example.Chat.Server.Implementation
{
    public class Room : IRoom
    {
        public ChatRoom ChatRoom { get; }

        private readonly IMessageTracker messageTracker;
        private readonly ConcurrentDictionary<string, IClient> clients;
        private readonly Channel<(IMessage, IClient)> messages;
        private readonly IRoomBackplane backplane;

        public Room(ChatRoom room, IMessageTracker messageTracker, IRoomBackplane backplane)
        {
            ChatRoom = room;
            this.messageTracker = messageTracker;
            this.backplane = backplane;
            this.clients = new ConcurrentDictionary<string, IClient>();
            this.messages = Channel.CreateUnbounded<(IMessage, IClient)>();
        }

        public Task AddMessage(IMessage message, IClient client, CancellationToken token = default)
        {
            return messages.Writer.WriteAsync((message, client), token).AsTask();
        }

        public Task AddUser(IClient client, CancellationToken token = default)
        {
            if (!clients.TryAdd(client.User.Id, client))
            {
                throw new ChatException($"There already was a [user={client.User}] in [room={this}]");
            }
            
            return Task.CompletedTask;
        }

        public Task RemoveUser(IClient client, CancellationToken token = default)
        {
            if (!clients.TryRemove(client.User.Id, out _))
            {
                throw new ChatException($"There was no [user={client.User}] in [room={this}]");
            }

            return Task.CompletedTask;
        }

        private IEnumerable<IChatRoomUser> ListUsers()
        {
            return clients.Values.Select(client => new ChatRoomUser(ChatRoom, client));
        }

        public Task<IReadOnlyCollection<ChatMessage>> MostRecentMessages(uint maxMessages, 
                                                                         CancellationToken token = default)
        {
            return messageTracker.LastMessages(maxMessages, token);
        }

        public async Task RunAsync(CancellationToken token = default)
        {
            await Task.WhenAll(ReadLocalMessages(token), ReadBackplaneMessages(token));
        }

        private async Task ReadLocalMessages(CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                var (message, client) = await messages.Reader.ReadAsync(token);

                await DispatchMessage(message, client, token);
            }
        }

        private async Task ReadBackplaneMessages(CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                var message = await backplane.ReceiveMessage(token);

                var sendTasks = clients.Values.Select(chatClient => chatClient.SendMessage(message, token));
                await Task.WhenAll(sendTasks);
            }
        }

        private Task DispatchMessage(IMessage message,
                                     IClient client,
                                     CancellationToken token = default)
        {
            return message switch
            {
                ExchangeMessage exchangeMessage => HandleRequest(exchangeMessage, client, token),
                IChatRoomMessage chatRoomMessage => HandleMessage(chatRoomMessage, token),
                _ => Task.CompletedTask
            };
        }

        private async Task HandleRequest(ExchangeMessage exchangeMessage,
                                         IClient client,
                                         CancellationToken token = default)
        {
            switch (exchangeMessage.Message)
            {
                case ListUsersRequest:
                {
                    var users = ListUsers().Select(user => user.User).ToList();
                    var response = exchangeMessage.MakeResponse(new ListUsersResponse(ChatRoom, users));
                    await client.SendMessage(response, token);
                    break;
                }
            }
        }

        private async Task HandleMessage(IChatRoomMessage chatRoomMessage, CancellationToken token = default)
        {
            switch (chatRoomMessage)
            {
                case ChatMessage chatMessage:
                {
                    await messageTracker.TrackMessage(chatMessage, token);
                    await backplane.AddMessage(chatMessage, token);
                    break;
                }
            }
        }

        private class ChatRoomUser : IChatRoomUser
        {
            public ChatUser User => Client.User;
            public ChatRoom Room { get; }
            public IClient Client { get; }

            public ChatRoomUser(ChatRoom room, IClient client)
            {
                Room = room;
                Client = client;
            }
        }
    }
}
