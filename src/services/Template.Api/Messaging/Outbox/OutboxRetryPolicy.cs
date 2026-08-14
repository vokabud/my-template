namespace Template.Api.Messaging.Outbox;

internal static class OutboxRetryPolicy
{
    private const int MaxErrorLength = 4000;
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromMinutes(5);

    internal static TimeSpan GetDelay(int attempts)
    {
        var delay = TimeSpan.FromSeconds(Math.Pow(2, Math.Min(attempts, 8)));
        return delay <= MaxRetryDelay ? delay : MaxRetryDelay;
    }

    internal static string TruncateError(string value)
    {
        return value.Length <= MaxErrorLength ? value : value[..MaxErrorLength];
    }
}
