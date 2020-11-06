using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestCache
{
    class ConsoleLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return new NoopDisposable();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string message = string.Empty;
            if (formatter != null)
            {
                message += formatter(state, exception);
            }
            Console.WriteLine($"{logLevel} - {eventId.Id} - {message}");

        }

        private class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
