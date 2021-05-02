using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Critical.Chat.Protocol;
using Critical.Chat.Protocol.Messages;
using Critical.Chat.Protocol.Transport;

namespace Critical.Chat.Server.Implementation
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
                if (message is IChatRoomMessage chatRoomMessage)
                {
                    if (rooms.TryGetValue(chatRoomMessage.Room.Id, out var chatRoom))
                    {
                        await chatRoom.MessagesSink.WriteAsync((chatRoomMessage, this), token);
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
            if (!rooms.TryRemove(serverChatRoom.Id, out var _))
            {
                throw new Exception($"Failed to leave [room={serverChatRoom}][client={this}]");
            }
        }
    }
}
