using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Lemvik.Example.Chat.Protocol.Messages
{
    public interface IMessageProtocol
    {
        Task<IMessage> Parse(Stream sourceStream, CancellationToken token = default);
        Task Serialize(IMessage message, Stream targetStream, CancellationToken token = default);
    }
}
