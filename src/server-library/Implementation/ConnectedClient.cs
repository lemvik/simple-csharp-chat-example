using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using Lemvik.Example.Chat.Protocol.Messages;
using Lemvik.Example.Chat.Protocol.Transport;

namespace Lemvik.Example.Chat.Server.Implementation
{
    public class ConnectedClient : IConnectedClient
    {
        public IChatUser User { get; }
        private readonly IChatTransport transport;
        private readonly ChannelWriter<(IConnectedClient, IMessage)> serverSink;
        private readonly ConcurrentDictionary<string, IServerChatRoom> rooms;

        public ConnectedClient(IChatUser user,
                               IChatTransport transport,
                               ChannelWriter<(IConnectedClient, IMessage)> serverSink)
        {
            User = user;
            this.transport = transport;
            this.serverSink = serverSink;
            this.rooms = new ConcurrentDictionary<string, IServerChatRoom>();
        }

        public async Task RunAsync(CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                var message = await transport.Receive(token);
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
                        await chatRoom.MessagesSink.WriteAsync((message, this), token);
                    }
                    else
                    {
                        // TODO: log error and ignore message.
                    }
                }
                else
                {
                    await serverSink.WriteAsync((this, message), token);
                }
            }
        }

        public Task SendMessage(IMessage message, CancellationToken token = default)
        {
            return transport.Send(message, token);
        }

        public void EnterRoom(IServerChatRoom serverChatRoom)
        {
            if (!rooms.TryAdd(serverChatRoom.Id, serverChatRoom))
            {
                throw new Exception($"Failed to enter [room={serverChatRoom}][client={this}]");
            }
        }

        public void LeaveRoom(IServerChatRoom serverChatRoom)
        {
            if (!rooms.TryRemove(serverChatRoom.Id, out _))
            {
                throw new Exception($"Failed to leave [room={serverChatRoom}][client={this}]");
            }
        }
    }
}
