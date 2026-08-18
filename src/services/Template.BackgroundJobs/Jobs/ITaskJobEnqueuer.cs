namespace Template.BackgroundJobs.Jobs;

public interface ITaskJobEnqueuer
{
    string Enqueue(Guid taskId);
}
