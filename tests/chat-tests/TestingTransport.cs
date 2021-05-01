using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Critical.Chat.Protocol.Messages;
using Critical.Chat.Protocol.Transport;

namespace Critical.Chat.Testing
{
    public class TestingTransport : IChatTransport
    {
        private readonly ChannelWriter<IMessage> sendingSink;
        private readonly ChannelReader<IMessage> receivingSource;

        public TestingTransport(ChannelWriter<IMessage> sendingSink, ChannelReader<IMessage> receivingSource)
        {
            this.sendingSink = sendingSink;
            this.receivingSource = receivingSource;
        }

        public Task Send(IMessage message, CancellationToken token = default)
        {
            return sendingSink.WriteAsync(message, token).AsTask();
        }

        public Task<IMessage> Receive(CancellationToken token = default)
        {
            return receivingSource.ReadAsync(token).AsTask();
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
