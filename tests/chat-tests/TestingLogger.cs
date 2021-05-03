using System;
using Microsoft.Extensions.Logging;

namespace Lemvik.Example.Chat.Testing
{
    internal static class TestingLogger
    {
        public static ILoggerFactory CreateFactory()
        {
            return new ConsoleLoggerFactory();
        }

        private class ConsoleLoggerFactory : ILoggerFactory
        {
            public void Dispose()
            {
            }

            public ILogger CreateLogger(string categoryName)
            {
                return new ConsoleLogger<object>();
            }

            public void AddProvider(ILoggerProvider provider)
            {
            }
        }

        private class ConsoleLogger<T> : ILogger<T>, IDisposable
        {
            public void Log<TState>(LogLevel logLevel,
                                    EventId eventId,
                                    TState state,
                                    Exception exception,
                                    Func<TState, Exception, string> formatter)
            {
                Console.WriteLine(exception != null
                                      ? $"{formatter(state, exception)}: {exception.Message} - {exception.StackTrace}"
                                      : formatter(state, null));
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return this;
            }

            public void Dispose()
            {
            }
        }
    }
}
