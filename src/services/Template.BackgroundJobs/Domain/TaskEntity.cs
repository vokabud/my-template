namespace Template.BackgroundJobs.Domain;

public sealed class TaskEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public TaskStatus Status { get; set; } = TaskStatus.Pending;

    public DateTimeOffset? ProcessedAt { get; set; }
}
