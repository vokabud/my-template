using FluentAssertions;
using Microsoft.Extensions.Logging;
using Template.BackgroundJobs.Jobs;
using Template.BackgroundJobs.Persistence;
using Xunit;

namespace Template.BackgroundJobs.UnitTests.Jobs;

public sealed class TaskProcessingJobTests
{
    [Fact]
    public async Task ExecuteAsync_logs_the_task_when_this_invocation_processes_it()
    {
        var id = Guid.NewGuid();
        var now = new DateTimeOffset(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);
        var store = new RecordingTaskStore(new ProcessedTask(id, "Import invoices"));
        var logger = new RecordingLogger<TaskProcessingJob>();
        var sut = new TaskProcessingJob(store, new FixedTimeProvider(now), logger);

        await sut.ExecuteAsync(id, CancellationToken.None);

        store.ProcessedId.Should().Be(id);
        store.ProcessedAt.Should().Be(now);
        logger.Messages.Should().ContainSingle()
            .Which.Should().ContainAll(id.ToString(), "Import invoices");
    }

    [Fact]
    public async Task ExecuteAsync_does_not_log_when_the_task_was_not_processed()
    {
        var store = new RecordingTaskStore(null);
        var logger = new RecordingLogger<TaskProcessingJob>();
        var sut = new TaskProcessingJob(
            store,
            new FixedTimeProvider(DateTimeOffset.UnixEpoch),
            logger);

        await sut.ExecuteAsync(Guid.NewGuid(), CancellationToken.None);

        logger.Messages.Should().BeEmpty();
    }

    private sealed class RecordingTaskStore(ProcessedTask? result) : ITaskStore
    {
        public Guid? ProcessedId { get; private set; }
        public DateTimeOffset? ProcessedAt { get; private set; }

        public Task<IReadOnlyList<Guid>> GetPendingIdsAsync(
            int batchSize,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<Template.BackgroundJobs.Domain.TaskEntity>> GetAllAsync(
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<ProcessedTask?> TryProcessAsync(
            Guid id,
            DateTimeOffset processedAt,
            CancellationToken cancellationToken)
        {
            ProcessedId = id;
            ProcessedAt = processedAt;
            return Task.FromResult(result);
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

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
                Messages.Add(formatter(state, exception));
        }
    }
}
