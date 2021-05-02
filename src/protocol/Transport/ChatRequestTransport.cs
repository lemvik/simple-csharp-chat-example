using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Critical.Chat.Protocol.Messages;

namespace Critical.Chat.Protocol.Transport
{
    public class ChatRequestTransport : IChatRequestTransport
    {
        private readonly IChatTransport transport;
        private readonly ConcurrentDictionary<ulong, TaskCompletionSource<IResponse>> pendingMessages;
        private ulong sequence;

        public ChatRequestTransport(IChatTransport transport)
        {
            this.transport = transport;
            this.pendingMessages = new ConcurrentDictionary<ulong, TaskCompletionSource<IResponse>>();
        }

        public Task Send(IMessage message, CancellationToken token = default)
        {
            return transport.Send(message, token);
        }

        public async Task<IMessage> Receive(CancellationToken token = default)
        {
            while (true)
            {
                var message = await transport.Receive(token);
                if (message is IResponse response &&
                    pendingMessages.TryRemove(response.RequestId, out var waitingTask))
                {
                    waitingTask.SetResult(response);
                }
                else
                {
                    return message;
                }
            }
        }

        public async Task<TResponse> Exchange<TResponse>(IRequest request, CancellationToken token = default)
            where TResponse : IMessage
        {
            request.RequestId = ++sequence;
            var waitingTask = new TaskCompletionSource<IResponse>();
            if (!pendingMessages.TryAdd(request.RequestId, waitingTask))
            {
                throw new Exception($"Failed to schedule exchange [request={request}]");
            }

            await transport.Send(request, token);

            using (token.Register(() => waitingTask.TrySetCanceled()))
            {
                var taskResponse = await waitingTask.Task;
                if (taskResponse is TResponse response)
                {
                    return response;
                }

                throw new Exception($"Received unexpected [request={request}][response={taskResponse}]");
            }
        }
    }
}
