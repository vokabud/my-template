using Hangfire;

namespace Template.BackgroundJobs.Jobs;

public sealed class HangfireTaskJobEnqueuer(IBackgroundJobClient backgroundJobClient)
    : ITaskJobEnqueuer
{
    public string Enqueue(Guid taskId)
    {
        return backgroundJobClient.Enqueue<TaskProcessingJob>(
            job => job.ExecuteAsync(taskId, CancellationToken.None));
    }
}
