using System;

namespace Lemvik.Example.Chat.Protocol.Messages
{
    public static class MessageUtilities
    {
        public static TMessage Cast<TMessage>(this IMessage message) where TMessage : IMessage
        {
            if (message is TMessage castMessage)
            {
                return castMessage;
            }

            throw new InvalidCastException($"Expected [message={message}] to be of [type={typeof(TMessage)}]");
        }
    }
}
