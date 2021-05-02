using System;
using System.Threading;
using System.Threading.Tasks;
using Critical.Chat.Protocol.Messages;
using Critical.Chat.Protocol.Transport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Critical.Chat.Testing
{
    [TestClass]
    public class ExchangeTests
    {
        [TestMethod, Timeout(1000)]
        public async Task TestSuccessfulExchange()
        {
            var (wrappedTransport, remoteTransport) = TestingTransport.CreatePair();
            var testLifetime = new CancellationTokenSource();
            var exchangeTransport = new ChatRequestTransport(wrappedTransport);
            var respondingTask = Task.Run(async () =>
            {
                var request = await remoteTransport.Receive(testLifetime.Token);
                if (request is RequestMessage requestMessage)
                {
                    var responseMessage = new ResponseMessage() {RequestId = requestMessage.RequestId};
                    await remoteTransport.Send(responseMessage, testLifetime.Token);
                }
            }, testLifetime.Token);
            
            // This is needed as exchangeTransport requires someone to poll Receive in order to pump messages.
            var pumpingTask = Task.Run(async () =>
            {
                await exchangeTransport.Receive(testLifetime.Token);
            }, testLifetime.Token);

            var response = await exchangeTransport.Exchange<ResponseMessage>(new RequestMessage(), testLifetime.Token);
            Assert.IsNotNull(response);
            testLifetime.Cancel();

            try
            {
                await Task.WhenAll(respondingTask, pumpingTask);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private class RequestMessage : IRequest
        {
            public MessageType Type => MessageType.HandshakeRequest;
            public ulong RequestId { get; set; }
        }

        private class ResponseMessage : IResponse
        {
            public MessageType Type => MessageType.HandshakeResponse;
            public ulong RequestId { get; set; }
        }
    }
}
