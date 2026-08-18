using Template.BackgroundJobs.Persistence;

namespace Template.BackgroundJobs.Jobs;

public sealed class TaskProcessingJob(
    ITaskStore taskStore,
    TimeProvider timeProvider,
    ILogger<TaskProcessingJob> logger)
{
    public async Task ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var processedTask = await taskStore.TryProcessAsync(
            id,
            timeProvider.GetUtcNow(),
            cancellationToken);

        if (processedTask is not null)
        {
            logger.LogInformation(
                "Processed task {TaskId} {TaskName}",
                processedTask.Id,
                processedTask.Name);
        }
    }
}
