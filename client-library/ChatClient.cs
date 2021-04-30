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
        private readonly IDictionary<ulong, TaskCompletionSource<IMessage>> pendingMessages;
        private ulong sequence = 0;

        internal ChatClient(ILogger<ChatClient> logger, IChatTransport transport)
        {
            this.logger = logger;
            this.transport = transport;
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
                    logger.LogDebug("Dispatching [message={message}]", incomingMessage);
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
    }
}
