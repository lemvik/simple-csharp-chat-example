using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using Lemvik.Example.Chat.Protocol.Messages;

namespace Lemvik.Example.Chat.Server.Implementation
{
    public class InMemoryMessageTracker : IMessageTracker
    {
        private readonly IList<ChatMessage> messages = new List<ChatMessage>();
        
        public Task TrackMessage(ChatMessage chatMessage, CancellationToken token = default)
        {
            messages.Add(chatMessage);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<ChatMessage>> LastMessages(uint count, CancellationToken token = default)
        {
            var index = (int) Math.Max(messages.Count - count, 0);
            return Task.FromResult<IReadOnlyCollection<ChatMessage>>(messages.Skip(index).Take((int) count).ToList());
        }

        public static IMessageTrackerFactory Factory => new TrackerFactory();

        private class TrackerFactory : IMessageTrackerFactory
        {
            public Task<IMessageTracker> Create(ChatRoom room, CancellationToken token = default)
            {
                return Task.FromResult<IMessageTracker>(new InMemoryMessageTracker());
            }
        }
    }
}
