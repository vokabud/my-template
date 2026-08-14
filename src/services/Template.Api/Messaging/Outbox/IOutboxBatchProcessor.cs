namespace Template.Api.Messaging.Outbox;

internal interface IOutboxBatchProcessor
{
    Task ProcessPendingMessagesAsync(CancellationToken cancellationToken);
}
