using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol.Messages;
using Lemvik.Example.Chat.Shared;

namespace Lemvik.Example.Chat.Protocol.Transport
{
    public class ChatExchangeTransport : IChatExchangeTransport
    {
        private readonly IChatTransport transport;
        private readonly ConcurrentDictionary<ulong, TaskCompletionSource<IMessage>> pendingMessages;
        private readonly CancellationTokenSource exchangeLifetime;
        private ulong sequence;

        public ChatExchangeTransport(IChatTransport transport)
        {
            this.transport = transport;
            this.pendingMessages = new ConcurrentDictionary<ulong, TaskCompletionSource<IMessage>>();
            this.exchangeLifetime = new CancellationTokenSource();
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
            var exchangeToken = CancellationTokenSource.CreateLinkedTokenSource(exchangeLifetime.Token, token).Token;
            var exchangeMessage = new ExchangeMessage(++sequence, request);
            var waitingTask = new TaskCompletionSource<IMessage>();
            using (exchangeToken.Register(() => waitingTask.TrySetCanceled()))
            {
                if (!pendingMessages.TryAdd(exchangeMessage.ExchangeId, waitingTask))
                {
                    throw new ChatException($"Failed to schedule exchange [request={request}]");
                }

                await transport.Send(exchangeMessage, exchangeToken);

                var taskResponse = await waitingTask.Task;
                return taskResponse switch
                {
                    ExchangeMessage {Message: TResponse response} => response,
                    ExchangeMessage {Message: ChatErrorResponse errorResponse} =>
                        throw new ChatException(errorResponse.Description),
                    _ => throw new ChatException($"Received unexpected [request={request}][response={taskResponse}]")
                };
            }
        }
        
        public Task Close()
        {
            exchangeLifetime.Cancel();
            return transport.Close();
        }

    }
}
