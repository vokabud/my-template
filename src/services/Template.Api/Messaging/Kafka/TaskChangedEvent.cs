namespace Template.Api.Messaging.Kafka;

public sealed record TaskChangedEvent(string EventType, DateTime OccurredAtUtc, TaskSnapshot Task);
