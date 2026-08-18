using Microsoft.EntityFrameworkCore;
using Template.BackgroundJobs.Domain;
using BackgroundTaskStatus = Template.BackgroundJobs.Domain.TaskStatus;

namespace Template.BackgroundJobs.Persistence;

public sealed class EfTaskStore(ApplicationDbContext context) : ITaskStore
{
    public async Task<IReadOnlyList<Guid>> GetPendingIdsAsync(
        int batchSize,
        CancellationToken cancellationToken)
    {
        return await context.Tasks
            .AsNoTracking()
            .Where(task => task.Status == BackgroundTaskStatus.Pending)
            .OrderBy(task => task.Id)
            .Select(task => task.Id)
            .Take(batchSize)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TaskEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Tasks
            .AsNoTracking()
            .OrderBy(task => task.Name)
            .ThenBy(task => task.Id)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<ProcessedTask?> TryProcessAsync(
        Guid id,
        DateTimeOffset processedAt,
        CancellationToken cancellationToken)
    {
        var pendingTask = await context.Tasks
            .AsNoTracking()
            .Where(task => task.Id == id && task.Status == BackgroundTaskStatus.Pending)
            .Select(task => new ProcessedTask(task.Id, task.Name))
            .SingleOrDefaultAsync(cancellationToken);

        if (pendingTask is null)
            return null;

        var affectedRows = await context.Tasks
            .Where(task => task.Id == id && task.Status == BackgroundTaskStatus.Pending)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(task => task.Status, BackgroundTaskStatus.Processed)
                    .SetProperty(task => task.ProcessedAt, processedAt),
                cancellationToken);

        return affectedRows == 1 ? pendingTask : null;
    }
}
