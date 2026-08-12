using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Template.Api.Domain;
using Template.Api.Persistence;
using Template.ServiceDefaults.Messaging.Kafka;

namespace Template.Api.Messaging.Outbox;

public sealed class OutboxMessageProcessor : BackgroundService
{
    private const int BatchSize = 20;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromMinutes(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxMessageProcessor> _logger;

    public OutboxMessageProcessor(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxMessageProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);

        do
        {
            try
            {
                await ProcessPendingMessagesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to process pending outbox messages.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();
        var now = DateTime.UtcNow;

        var messages = await dbContext.OutboxMessages
            .Where(message => message.ProcessedAt == null
                && (message.NextAttemptAt == null || message.NextAttemptAt <= now))
            .OrderBy(message => message.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            await ProcessMessageAsync(dbContext, publisher, message, cancellationToken);
        }
    }

    private async Task ProcessMessageAsync(
        ApplicationDbContext dbContext,
        IMessagePublisher publisher,
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            if (message.Payload is null)
            {
                await publisher.PublishTombstoneAsync(message.Topic, message.Key, cancellationToken);
            }
            else
            {
                using var payload = JsonDocument.Parse(message.Payload);
                await publisher.PublishAsync(
                    message.Topic,
                    message.Key,
                    payload.RootElement.Clone(),
                    cancellationToken);
            }

            message.ProcessedAt = DateTime.UtcNow;
            message.LastError = null;

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            message.Attempts++;
            message.LastError = Truncate(ex.Message, 4000);
            message.NextAttemptAt = DateTime.UtcNow + GetRetryDelay(message.Attempts);

            _logger.LogWarning(
                ex,
                "Failed to publish outbox message {MessageId} to topic {Topic}. Attempt {Attempt}.",
                message.Id,
                message.Topic,
                message.Attempts);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static TimeSpan GetRetryDelay(int attempts)
    {
        var delay = TimeSpan.FromSeconds(Math.Pow(2, Math.Min(attempts, 8)));
        return delay <= MaxRetryDelay ? delay : MaxRetryDelay;
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
