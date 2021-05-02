using System.Threading.Tasks;
using Critical.Chat.Protocol.Transport;

namespace Critical.Chat.Protocol.Messages
{
    public class ExchangeMessage : IMessage
    {
        public ulong ExchangeId { get; }
        public IMessage Message { get; }

        public ExchangeMessage(ulong exchangeId, IMessage message)
        {
            ExchangeId = exchangeId;
            Message = message;
        }

        public ExchangeMessage MakeResponse(IMessage responsePayload)
        {
            return new(ExchangeId, responsePayload);
        }
    }
}
