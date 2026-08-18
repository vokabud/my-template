using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Template.BackgroundJobs.Domain;
using Template.BackgroundJobs.Endpoints.Tasks;
using Template.BackgroundJobs.Persistence;
using Xunit;
using BackgroundTaskStatus = Template.BackgroundJobs.Domain.TaskStatus;

namespace Template.BackgroundJobs.UnitTests.Endpoints.Tasks;

public sealed class GetTasksHandlerTests
{
    [Fact]
    public async Task Handle_returns_the_read_only_task_contract()
    {
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var processedAt = new DateTimeOffset(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);
        var store = new QueryTaskStore([
            new TaskEntity { Id = firstId, Name = "Alpha", Status = BackgroundTaskStatus.Pending },
            new TaskEntity
            {
                Id = secondId,
                Name = "Beta",
                Status = BackgroundTaskStatus.Processed,
                ProcessedAt = processedAt
            }
        ]);

        Ok<TaskResponse[]> result = await GetTasksHandler.Handle(store, CancellationToken.None);

        result.Value.Should().Equal(
            new TaskResponse(firstId, "Alpha", BackgroundTaskStatus.Pending, null),
            new TaskResponse(secondId, "Beta", BackgroundTaskStatus.Processed, processedAt));
    }

    [Fact]
    public async Task Handle_returns_an_empty_array_when_no_tasks_exist()
    {
        var result = await GetTasksHandler.Handle(new QueryTaskStore([]), CancellationToken.None);

        result.Value.Should().BeEmpty();
    }

    private sealed class QueryTaskStore(IReadOnlyList<TaskEntity> tasks) : ITaskStore
    {
        public Task<IReadOnlyList<TaskEntity>> GetAllAsync(CancellationToken cancellationToken) =>
            Task.FromResult(tasks);

        public Task<IReadOnlyList<Guid>> GetPendingIdsAsync(
            int batchSize,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<ProcessedTask?> TryProcessAsync(
            Guid id,
            DateTimeOffset processedAt,
            CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
