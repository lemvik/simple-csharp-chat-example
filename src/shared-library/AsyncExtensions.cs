using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Lemvik.Example.Chat.Shared
{
    public static class AsyncExtensions
    {
        public static CancellationTokenAwaiter GetAwaiter(this CancellationToken token)
        {
            return new CancellationTokenAwaiter(token);
        }
        
        public class CancellationTokenAwaiter : ICriticalNotifyCompletion
        {
            private CancellationToken token;

            public CancellationTokenAwaiter(CancellationToken token)
            {
                this.token = token;
            }

            public bool IsCompleted => token.IsCancellationRequested;

            public object GetResult()
            {
                if (IsCompleted)
                {
                    return true;
                }

                throw new InvalidOperationException("GetResult() called on non-completed awaiter");
            }
            
            public void OnCompleted(Action continuation)
            {
                token.Register(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                token.Register(continuation);
            }
        } 
    }
}
