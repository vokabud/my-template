namespace Template.Api.Messaging.Kafka;

public interface ITaskEventPublisher
{
    Task PublishAsync(string eventType, TaskSnapshot task, CancellationToken cancellationToken);
}
