using BackgroundTaskStatus = Template.BackgroundJobs.Domain.TaskStatus;

namespace Template.BackgroundJobs.Endpoints.Tasks;

public sealed record TaskResponse(
    Guid Id,
    string Name,
    BackgroundTaskStatus Status,
    DateTimeOffset? ProcessedAt);
