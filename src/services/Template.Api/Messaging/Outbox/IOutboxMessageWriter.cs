namespace Template.Api.Messaging.Outbox;

public interface IOutboxMessageWriter
{
    void AddMessage(string topic, Guid key, object payload);

    void AddTombstone(string topic, Guid key);
}
