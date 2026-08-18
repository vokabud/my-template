using FluentAssertions;
using Microsoft.Extensions.Options;
using Template.BackgroundJobs.Domain;
using Template.BackgroundJobs.Jobs;
using Template.BackgroundJobs.Persistence;
using Xunit;

namespace Template.BackgroundJobs.UnitTests.Jobs;

public sealed class TaskDispatcherJobTests
{
    [Fact]
    public async Task ExecuteAsync_enqueues_each_selected_pending_task()
    {
        var ids = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var store = new PendingTaskStore(ids);
        var enqueuer = new RecordingEnqueuer();
        var sut = new TaskDispatcherJob(
            store,
            enqueuer,
            Options.Create(new BackgroundJobOptions { DispatcherBatchSize = 100 }));

        await sut.ExecuteAsync(CancellationToken.None);

        store.RequestedBatchSize.Should().Be(100);
        enqueuer.TaskIds.Should().Equal(ids);
    }

    [Fact]
    public async Task ExecuteAsync_does_not_enqueue_when_no_tasks_are_pending()
    {
        var enqueuer = new RecordingEnqueuer();
        var sut = new TaskDispatcherJob(
            new PendingTaskStore([]),
            enqueuer,
            Options.Create(new BackgroundJobOptions()));

        await sut.ExecuteAsync(CancellationToken.None);

        enqueuer.TaskIds.Should().BeEmpty();
    }

    private sealed class PendingTaskStore(IReadOnlyList<Guid> ids) : ITaskStore
    {
        public int? RequestedBatchSize { get; private set; }

        public Task<IReadOnlyList<Guid>> GetPendingIdsAsync(
            int batchSize,
            CancellationToken cancellationToken)
        {
            RequestedBatchSize = batchSize;
            return Task.FromResult(ids);
        }

        public Task<IReadOnlyList<TaskEntity>> GetAllAsync(
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<ProcessedTask?> TryProcessAsync(
            Guid id,
            DateTimeOffset processedAt,
            CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class RecordingEnqueuer : ITaskJobEnqueuer
    {
        public List<Guid> TaskIds { get; } = [];

        public string Enqueue(Guid taskId)
        {
            TaskIds.Add(taskId);
            return taskId.ToString();
        }
    }
}
