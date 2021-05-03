using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol.Messages;
using Lemvik.Example.Chat.Protocol.Transport;

namespace Lemvik.Example.Chat.Testing
{
    public class TestingTransport : IChatTransport
    {
        private readonly ChannelWriter<IMessage> sendingSink;
        private readonly ChannelReader<IMessage> receivingSource;
        private readonly CancellationTokenSource transportLifetime;

        private TestingTransport(ChannelWriter<IMessage> sendingSink, ChannelReader<IMessage> receivingSource)
        {
            this.sendingSink = sendingSink;
            this.receivingSource = receivingSource;
            this.transportLifetime = new CancellationTokenSource();
        }

        public Task Send(IMessage message, CancellationToken token = default)
        {
            var operationToken = CancellationTokenSource.CreateLinkedTokenSource(transportLifetime.Token, token).Token;
            return sendingSink.WriteAsync(message, operationToken).AsTask();
        }

        public Task<IMessage> Receive(CancellationToken token = default)
        {
            var operationToken = CancellationTokenSource.CreateLinkedTokenSource(transportLifetime.Token, token).Token;
            return receivingSource.ReadAsync(operationToken).AsTask();
        }

        public void Close()
        {
            sendingSink.Complete();
        }

        public static (TestingTransport, TestingTransport) CreatePair()
        {
            var forward = Channel.CreateUnbounded<IMessage>();
            var backward = Channel.CreateUnbounded<IMessage>();

            return (new TestingTransport(forward.Writer, backward.Reader),
                new TestingTransport(backward.Writer, forward.Reader));
        }
    }
}
