namespace Template.ServiceDefaults.Messaging.Kafka;

public interface IMessagePublisher
{
    Task PublishAsync(
        string topic,
        Guid id,
        object body,
        CancellationToken cancellationToken);

    Task PublishTombstoneAsync(
        string topic,
        Guid id,
        CancellationToken cancellationToken);
}
