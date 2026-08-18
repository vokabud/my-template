using Template.BackgroundJobs.Domain;

namespace Template.BackgroundJobs.Persistence;

public sealed record ProcessedTask(Guid Id, string Name);

public interface ITaskStore
{
    Task<IReadOnlyList<Guid>> GetPendingIdsAsync(
        int batchSize,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<TaskEntity>> GetAllAsync(CancellationToken cancellationToken);

    Task<ProcessedTask?> TryProcessAsync(
        Guid id,
        DateTimeOffset processedAt,
        CancellationToken cancellationToken);
}
