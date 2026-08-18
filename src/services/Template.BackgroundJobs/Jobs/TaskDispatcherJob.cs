using Hangfire;
using Microsoft.Extensions.Options;
using Template.BackgroundJobs.Persistence;

namespace Template.BackgroundJobs.Jobs;

public sealed class TaskDispatcherJob(
    ITaskStore taskStore,
    ITaskJobEnqueuer taskJobEnqueuer,
    IOptions<BackgroundJobOptions> options)
{
    [DisableConcurrentExecution(timeoutInSeconds: 50)]
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var taskIds = await taskStore.GetPendingIdsAsync(
            options.Value.DispatcherBatchSize,
            cancellationToken);

        foreach (var taskId in taskIds)
            taskJobEnqueuer.Enqueue(taskId);
    }
}
