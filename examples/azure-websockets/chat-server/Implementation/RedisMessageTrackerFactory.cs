using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Protocol;
using StackExchange.Redis;

namespace Lemvik.Example.Chat.Server.Examples.Azure.Implementation
{
    public class RedisMessageTrackerFactory : IMessageTrackerFactory
    {
        private readonly IDatabaseAsync database;

        public RedisMessageTrackerFactory(IDatabaseAsync database)
        {
            this.database = database;
        }

        public Task<IMessageTracker> Create(ChatRoom room, CancellationToken token = default)
        {
            return Task.FromResult<IMessageTracker>(new RedisMessageTracker(room, database));
        }
    }
}
