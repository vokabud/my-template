namespace Template.Api.Messaging.Outbox;

public sealed class OutboxMessageProcessor : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxMessageProcessor> _logger;

    public OutboxMessageProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxMessageProcessor> logger)
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
                using var scope = _scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IOutboxBatchProcessor>();
                await processor.ProcessPendingMessagesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to process pending outbox messages.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
