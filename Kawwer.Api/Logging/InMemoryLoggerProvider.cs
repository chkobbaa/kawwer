using Microsoft.Extensions.Logging;

namespace Kawwer.Api.Logging;

/// <summary>
/// An <see cref="ILoggerProvider"/> that mirrors every emitted log entry into the shared
/// <see cref="InMemoryLogStore"/> so the password-gated logs viewer can read recent activity.
/// </summary>
public sealed class InMemoryLoggerProvider : ILoggerProvider
{
    private readonly InMemoryLogStore _store;

    public InMemoryLoggerProvider(InMemoryLogStore store) => _store = store;

    public ILogger CreateLogger(string categoryName) => new InMemoryLogger(categoryName, _store);

    public void Dispose()
    {
    }

    private sealed class InMemoryLogger : ILogger
    {
        private readonly string _category;
        private readonly InMemoryLogStore _store;

        public InMemoryLogger(string category, InMemoryLogStore store)
        {
            _category = category;
            _store = store;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message) && exception is null)
            {
                return;
            }

            _store.Add(
                DateTime.UtcNow,
                logLevel.ToString(),
                _category,
                message,
                exception?.ToString());
        }
    }
}
