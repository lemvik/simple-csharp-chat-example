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
            var captured = new CapturedRunnable(id, runnable, lifetime);
            if (!trackedRunnables.TryAdd(id, captured))
            {
                return false;
            }

            RunTracked(captured);
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

        public bool TryRemoveAndStop(TId id, out Task runningTask)
        {
            if (!trackedRunnables.TryRemove(id, out var captured))
            {
                runningTask = Task.CompletedTask;
                return false;
            }

            runningTask = captured.Task;
            captured.Lifetime.Cancel();
            return true;
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

        private void RunTracked(CapturedRunnable runnable)
        {
            runnable.Task = runnable.Tracked.RunAsync(runnable.Lifetime.Token).ContinueWith(result =>
            {
                trackedRunnables.TryRemove(runnable.Id, out _);
            });
        }

        private class CapturedRunnable
        {
            public TId Id { get; }
            public TTracked Tracked { get; }
            public Task Task { get; set; }
            public CancellationTokenSource Lifetime { get; }

            public CapturedRunnable(TId id, TTracked tracked, CancellationTokenSource lifetime)
            {
                Id = id;
                Tracked = tracked;
                Lifetime = lifetime;
            }
        }
    }
}
