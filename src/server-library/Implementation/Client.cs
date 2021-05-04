using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using Lemvik.Example.Chat.Protocol.Messages;
using Lemvik.Example.Chat.Protocol.Transport;
using Microsoft.Extensions.Logging;

namespace Lemvik.Example.Chat.Server.Implementation
{
    internal class Client : IClient
    {
        public ChatUser User { get; }
        public IEnumerable<IRoom> Rooms => rooms.Values;
        private readonly ILogger<Client> logger;
        private readonly IChatTransport transport;
        private readonly ChannelWriter<(Client, IMessage)> serverSink;
        private readonly ConcurrentDictionary<string, IRoom> rooms;

        public Client(ILogger<Client> logger,
                      ChatUser user,
                      IChatTransport transport,
                      ChannelWriter<(Client, IMessage)> serverSink)
        {
            User = user;
            this.logger = logger;
            this.transport = transport;
            this.serverSink = serverSink;
            this.rooms = new ConcurrentDictionary<string, IRoom>();
        }

        public async Task RunAsync(CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                var message = await transport.Receive(token);
                await DispatchMessage(message, token);
            }
        }

        private async Task DispatchMessage(IMessage message, CancellationToken token = default)
        {
            var targetRoom = message switch
            {
                IChatRoomMessage chatRoomMessage => chatRoomMessage.Room,
                ExchangeMessage {Message: IChatRoomMessage chatRoomMessage} => chatRoomMessage.Room,
                _ => default
            };

            if (targetRoom != null)
            {
                if (rooms.TryGetValue(targetRoom.Id, out var chatRoom))
                {
                    await chatRoom.AddMessage(message, this, token);
                }
                else
                {
                    logger.LogWarning("Cannot dispatch [message={Message}] to unknown [room={Room}]",
                                      message,
                                      targetRoom);
                }
            }
            else
            {
                await serverSink.WriteAsync((this, message), token);
            }
        }

        public Task SendMessage(IMessage message, CancellationToken token = default)
        {
            return transport.Send(message, token);
        }

        public void EnterRoom(IRoom room)
        {
            if (!rooms.TryAdd(room.ChatRoom.Id, room))
            {
                throw new Exception($"Failed to enter [room={room}][client={this}]");
            }
        }

        public void LeaveRoom(IRoom room)
        {
            if (!rooms.TryRemove(room.ChatRoom.Id, out _))
            {
                throw new Exception($"Failed to leave [room={room}][client={this}]");
            }
        }

        public override string ToString()
        {
            return $"Client[User={User},Transport={transport},Rooms={rooms.Count}]";
        }
    }
}
