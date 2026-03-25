using Template.Api.Domain;

namespace Template.Api.Messaging.Kafka;

public sealed record TaskSnapshot(Guid Id, string Name, string Description)
{
    public static TaskSnapshot FromEntity(TaskEntity task) => new(task.Id, task.Name, task.Description);
}
