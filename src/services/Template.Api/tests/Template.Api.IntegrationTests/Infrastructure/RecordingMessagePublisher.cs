using System.Collections.Concurrent;
using Template.ServiceDefaults.Messaging.Kafka;

namespace Template.Api.IntegrationTests.Infrastructure;

internal sealed class RecordingMessagePublisher : IMessagePublisher
{
    private readonly ConcurrentQueue<PublishedMessage> _messages = new();
    internal Exception? Failure { get; set; }
    internal IReadOnlyCollection<PublishedMessage> Messages => _messages.ToArray();

    public Task PublishAsync(string topic, Guid id, object body, CancellationToken cancellationToken)
    {
        if (Failure is not null) throw Failure;
        _messages.Enqueue(new(topic, id, body, false));
        return Task.CompletedTask;
    }

    public Task PublishTombstoneAsync(string topic, Guid id, CancellationToken cancellationToken)
    {
        if (Failure is not null) throw Failure;
        _messages.Enqueue(new(topic, id, null, true));
        return Task.CompletedTask;
    }

    internal void Clear()
    {
        while (_messages.TryDequeue(out _)) { }
        Failure = null;
    }

    internal sealed record PublishedMessage(string Topic, Guid Key, object? Body, bool IsTombstone);
}
