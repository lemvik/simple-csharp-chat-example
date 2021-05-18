using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lemvik.Example.Chat.Shared
{
    public class TaskTracker
    {
        private readonly List<Task> tasks = new List<Task>();
        
        public void Run(Task tracked, CancellationToken token)
        {
            var wrapped = Task.Run(() => tracked, token);
            tasks.Add(wrapped);
            wrapped.ContinueWith(_ => tasks.Remove(wrapped), token);
        }

        public Task CloseAsync()
        {
            return Task.WhenAll(tasks);
        }
    }
}
