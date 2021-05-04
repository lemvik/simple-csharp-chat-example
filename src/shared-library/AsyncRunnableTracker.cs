using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lemvik.Example.Chat.Shared
{
    public class AsyncRunnableTracker<TId, TTracked> where TTracked : IAsyncRunnable
    {
        private readonly ConcurrentDictionary<TId, CapturedRunnable> trackedRunnables;
        private readonly CancellationTokenSource trackerLifetime;

        public AsyncRunnableTracker(CancellationToken parentLifetime)
        {
            this.trackedRunnables = new ConcurrentDictionary<TId, CapturedRunnable>();
            this.trackerLifetime = CancellationTokenSource.CreateLinkedTokenSource(parentLifetime);
        }

        public bool TryAdd(TId id, TTracked runnable, CancellationToken token = default)
        {
            var lifetime = CancellationTokenSource.CreateLinkedTokenSource(trackerLifetime.Token, token);
            var captured = new CapturedRunnable(runnable, lifetime);
            if (!trackedRunnables.TryAdd(id, captured))
            {
                return false;
            }

            captured.Task = captured.Tracked.RunAsync(lifetime.Token);
            return true;
        }

        public bool TryGet(TId id, out TTracked runnable)
        {
            runnable = default;
            if (!trackedRunnables.TryGetValue(id, out var captured))
            {
                return false;
            }

            runnable = captured.Tracked;
            return true;
        }

        public async Task<TTracked> StopAndRemoveIfTracked(TId id)
        {
            if (!trackedRunnables.TryRemove(id, out var captured))
            {
                return default;
            }

            captured.Lifetime.Cancel();
            await captured.Task;
            return captured.Tracked;
        }

        public IReadOnlyCollection<TTracked> GetAll()
        {
            return trackedRunnables.Values.Select(captured => captured.Tracked).ToList();
        }

        public async Task StopTracker()
        {
            var tracked = trackedRunnables.Values; 
            trackedRunnables.Clear();
            
            foreach (var capturedRunnable in tracked)
            {
                capturedRunnable.Lifetime.Cancel();
            }

            await Task.WhenAll(tracked.Select(captured => captured.Task));
        }

        private class CapturedRunnable
        {
            public TTracked Tracked { get; }
            public Task Task { get; set; }
            public CancellationTokenSource Lifetime { get; }

            public CapturedRunnable(TTracked tracked, CancellationTokenSource lifetime)
            {
                Tracked = tracked;
                Lifetime = lifetime;
            }
        }
    }
}
