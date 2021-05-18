using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol.Messages;
using Google.Protobuf;
using Lemvik.Example.Chat.Shared;
using IMessage = Lemvik.Example.Chat.Protocol.Messages.IMessage;

namespace Lemvik.Example.Chat.Protocol.Protobuf
{
    public class ProtobufMessageProtocol : IMessageProtocol
    {
        public async Task<IMessage> Parse(Stream sourceStream, CancellationToken token = default)
        {
            var messageBuffer = await sourceStream.ReadLengthPrefixedAsync(token);

            var message = ProtocolMessage.Parser.ParseFrom(messageBuffer);

            return message.FromProtocolMessage();
        }

        public Task Serialize(IMessage message, Stream targetStream, CancellationToken token = default)
        {
            var protocolMessage = message.ToProtocolMessage();
            var buffer = protocolMessage.ToByteArray();
            return targetStream.WriteLengthPrefixedAsync(buffer, token);
        }
    }
}
