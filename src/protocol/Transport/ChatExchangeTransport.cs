using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol.Messages;

namespace Lemvik.Example.Chat.Protocol.Transport
{
    public class ChatExchangeTransport : IChatExchangeTransport
    {
        private readonly IChatTransport transport;
        private readonly ConcurrentDictionary<ulong, TaskCompletionSource<IMessage>> pendingMessages;
        private ulong sequence;

        public ChatExchangeTransport(IChatTransport transport)
        {
            this.transport = transport;
            this.pendingMessages = new ConcurrentDictionary<ulong, TaskCompletionSource<IMessage>>();
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
                if (message is ExchangeMessage response &&
                    pendingMessages.TryRemove(response.ExchangeId, out var waitingTask))
                {
                    waitingTask.SetResult(response);
                }
                else
                {
                    return message;
                }
            }
        }

        public async Task<TResponse> Exchange<TResponse>(IMessage request, CancellationToken token = default)
            where TResponse : IMessage
        {
            var exchangeMessage = new ExchangeMessage(++sequence, request);
            var waitingTask = new TaskCompletionSource<IMessage>();
            using (token.Register(() => waitingTask.TrySetCanceled()))
            {
                if (!pendingMessages.TryAdd(exchangeMessage.ExchangeId, waitingTask))
                {
                    throw new Exception($"Failed to schedule exchange [request={request}]");
                }

                await transport.Send(exchangeMessage, token);

                var taskResponse = await waitingTask.Task;
                if (taskResponse is ExchangeMessage {Message: TResponse response})
                {
                    return response;
                }

                throw new Exception($"Received unexpected [request={request}][response={taskResponse}]");
            }
        }
    }
}
