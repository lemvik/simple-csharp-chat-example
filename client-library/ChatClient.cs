using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Critical.Chat.Protocol;
using Critical.Chat.Protocol.Messages;
using Critical.Chat.Protocol.Transport;
using Microsoft.Extensions.Logging;

namespace Critical.Chat.Client
{
    internal class ChatClient : IChatClient
    {
        private readonly ILogger<ChatClient> logger;
        private readonly IChatTransport transport;
        private readonly IChatClientConfiguration configuration;
        private readonly IDictionary<ulong, TaskCompletionSource<IMessage>> pendingMessages;
        private ulong sequence = 0;
        private string assignedId = string.Empty;

        internal ChatClient(ILogger<ChatClient> logger, 
                            IChatTransport transport,
                            IChatClientConfiguration configuration)
        {
            this.logger = logger;
            this.transport = transport;
            this.configuration = configuration;
            this.pendingMessages = new Dictionary<ulong, TaskCompletionSource<IMessage>>();
        }

        public async Task RunAsync(CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                var incomingMessage = await transport.Receive(token);

                if (pendingMessages.TryGetValue(incomingMessage.Id, out var pendingRequest))
                {
                    pendingMessages.Remove(incomingMessage.Id);
                    pendingRequest.SetResult(incomingMessage);
                }
                else
                {
                    await DispatchMessage(incomingMessage, token);
                }
            }
        }

        public async Task<IReadOnlyCollection<IChatRoom>> ListRooms(CancellationToken token = default)
        {
            var request = new ListRoomsRequest(++sequence);

            var response = await Exchange<ListRoomsResponse>(request, token);

            return response.Rooms;
        }

        public Task<IChatRoomUser> JoinRoom(IChatRoom room, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }

        private async Task<TResult> Exchange<TResult>(IMessage message, CancellationToken token = default)
            where TResult : class, IMessage
        {
            var completion = new TaskCompletionSource<IMessage>();
            pendingMessages.Add(message.Id, completion);

            await transport.Send(message, token);

            var response = await completion.Task;

            if (response is TResult result)
            {
                return result;
            }

            throw new Exception($"Received unexpected [response={response}] for [request={message}]");
        }

        private async Task DispatchMessage(IMessage message, CancellationToken token = default)
        {
            logger.LogDebug("Dispatching [message={message}]", message);

            switch (message.Type)
            {
                case MessageType.HandshakeRequest:
                {
                    var handshake = CastMessage<HandshakeRequest>(message);
                    assignedId = handshake.UserId;
                    var response = new HandshakeResponse(message.Id, configuration.UserName);
                    await transport.Send(response, token);
                    break;
                }
                case MessageType.HandshakeResponse:
                    break;
                case MessageType.ListRoomsRequest:
                    break;
                case MessageType.ListRoomsResponse:
                    break;
                case MessageType.CreateRoom:
                    break;
                case MessageType.JoinRoom:
                    break;
                case MessageType.LeaveRoom:
                    break;
                case MessageType.SendMessage:
                    break;
                case MessageType.ReceiveMessage:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private TMessage CastMessage<TMessage>(IMessage message) where TMessage : IMessage
        {
            if (message is TMessage castMessage)
            {
                return castMessage;
            }

            throw new Exception($"Invalid message type [message={message}][expected={typeof(TMessage)}]");
        }
    }
}
