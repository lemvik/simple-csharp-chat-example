namespace Lemvik.Example.Chat.Protocol.Messages
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
        
        public override string ToString()
        {
            return $"ExchangeMessage[ExchangeId={ExchangeId},Message={Message}]";
        }
    }
}
