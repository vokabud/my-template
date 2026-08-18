using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Template.BackgroundJobs.Jobs;

namespace Template.BackgroundJobs.IntegrationTests.Infrastructure;

internal sealed class RecordingTaskProcessingLogger : ILogger<TaskProcessingJob>
{
    private readonly ConcurrentQueue<string> _messages = new();

    internal IReadOnlyCollection<string> Messages => _messages.ToArray();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel == LogLevel.Information;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (IsEnabled(logLevel))
            _messages.Enqueue(formatter(state, exception));
    }

    internal void Clear()
    {
        while (_messages.TryDequeue(out _)) { }
    }
}
