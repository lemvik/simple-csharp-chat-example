using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using Lemvik.Example.Chat.Protocol.Messages;

namespace Lemvik.Example.Chat.Server.Implementation
{
    internal class Room : IRoom
    {
        public ChatRoom ChatRoom { get; }
        public ChannelWriter<(IMessage, IClient)> MessagesSink => messages.Writer;

        private readonly IMessageTracker messageTracker;
        private readonly ConcurrentDictionary<string, IClient> clients;
        private readonly Channel<(IMessage, IClient)> messages;

        internal Room(ChatRoom room, IMessageTracker messageTracker)
        {
            ChatRoom = room;
            this.messageTracker = messageTracker;
            this.clients = new ConcurrentDictionary<string, IClient>();
            this.messages = Channel.CreateUnbounded<(IMessage, IClient)>();
        }

        public Task AddUser(IClient client, CancellationToken token = default)
        {
            if (!clients.TryAdd(client.User.Id, client))
            {
                throw new ChatException($"There already was a [user={client.User}] in [room={this}]");
            }

            client.EnterRoom(this);

            return Task.CompletedTask;
        }

        public Task RemoveUser(IClient client, CancellationToken token = default)
        {
            if (!clients.TryRemove(client.User.Id, out _))
            {
                throw new ChatException($"There was no [user={client.User}] in [room={this}]");
            }

            client.LeaveRoom(this);

            return Task.CompletedTask;
        }

        private IReadOnlyCollection<IChatRoomUser> ListUsers()
        {
            var users = clients.Values.Select(client => new ChatRoomUser(ChatRoom, client)).ToList();
            return users;
        }

        public Task<IReadOnlyCollection<ChatMessage>> MostRecentMessages(uint maxMessages, 
                                                                         CancellationToken token = default)
        {
            return messageTracker.LastMessages(maxMessages, token);
        }

        public async Task RunAsync(CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                var (message, client) = await messages.Reader.ReadAsync(token);

                await DispatchMessage(message, client, token);
            }
        }

        private Task DispatchMessage(IMessage message,
                                     IClient client,
                                     CancellationToken token = default)
        {
            if (message is ExchangeMessage exchangeMessage)
            {
                return HandleRequest(exchangeMessage, client, token);
            }

            if (message is IChatRoomMessage chatRoomMessage)
            {
                return HandleMessage(chatRoomMessage, token);
            }

            return Task.CompletedTask;
        }

        private async Task HandleRequest(ExchangeMessage exchangeMessage,
                                         IClient client,
                                         CancellationToken token = default)
        {
            switch (exchangeMessage.Message)
            {
                case ListUsersRequest _:
                {
                    var users = ListUsers().Select(user => user.User).ToList();
                    var response = exchangeMessage.MakeResponse(new ListUsersResponse(ChatRoom, users));
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

        private async Task HandleMessage(IChatRoomMessage chatRoomMessage, CancellationToken token = default)
        {
            switch (chatRoomMessage)
            {
                case ChatMessage chatMessage:
                {
                    await messageTracker.TrackMessage(chatMessage, token);
                    var sendTasks = clients.Values.Select(chatClient => chatClient.SendMessage(chatMessage, token));
                    await Task.WhenAll(sendTasks);
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
