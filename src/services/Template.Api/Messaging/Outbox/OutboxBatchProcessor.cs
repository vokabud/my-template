using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Template.Api.Domain;
using Template.Api.Persistence;
using Template.ServiceDefaults.Messaging.Kafka;

namespace Template.Api.Messaging.Outbox;

internal sealed class OutboxBatchProcessor : IOutboxBatchProcessor
{
    private const int BatchSize = 20;
    private readonly ApplicationDbContext _dbContext;
    private readonly IMessagePublisher _publisher;
    private readonly ILogger<OutboxBatchProcessor> _logger;

    public OutboxBatchProcessor(ApplicationDbContext dbContext, IMessagePublisher publisher,
        ILogger<OutboxBatchProcessor> logger)
    {
        _dbContext = dbContext;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var messages = await _dbContext.OutboxMessages
            .Where(message => message.ProcessedAt == null
                && (message.NextAttemptAt == null || message.NextAttemptAt <= now))
            .OrderBy(message => message.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            await ProcessMessageAsync(message, cancellationToken);
        }
    }

    private async Task ProcessMessageAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        try
        {
            if (message.Payload is null)
                await _publisher.PublishTombstoneAsync(message.Topic, message.Key, cancellationToken);
            else
            {
                using var payload = JsonDocument.Parse(message.Payload);
                await _publisher.PublishAsync(message.Topic, message.Key, payload.RootElement.Clone(), cancellationToken);
            }

            message.ProcessedAt = DateTime.UtcNow;
            message.LastError = null;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            message.Attempts++;
            message.LastError = OutboxRetryPolicy.TruncateError(ex.Message);
            message.NextAttemptAt = DateTime.UtcNow + OutboxRetryPolicy.GetDelay(message.Attempts);
            _logger.LogWarning(ex,
                "Failed to publish outbox message {MessageId} to topic {Topic}. Attempt {Attempt}.",
                message.Id, message.Topic, message.Attempts);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
