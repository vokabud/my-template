using System.Text.Json;
using Template.Api.Domain;
using Template.Api.Persistence;

namespace Template.Api.Messaging.Outbox;

public sealed class OutboxMessageWriter : IOutboxMessageWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IApplicationDbContext _dbContext;

    public OutboxMessageWriter(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void AddMessage(string topic, Guid key, object payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        ArgumentNullException.ThrowIfNull(payload);

        _dbContext.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Topic = topic,
            Key = key,
            Payload = JsonSerializer.Serialize(payload, payload.GetType(), SerializerOptions),
            CreatedAt = DateTime.UtcNow
        });
    }

    public void AddTombstone(string topic, Guid key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);

        _dbContext.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Topic = topic,
            Key = key,
            CreatedAt = DateTime.UtcNow
        });
    }
}
