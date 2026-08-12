namespace Template.Api.Domain;

public class OutboxMessage
{
    public Guid Id { get; set; }

    public string Topic { get; set; } = string.Empty;

    public Guid Key { get; set; }

    public string? Payload { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime? NextAttemptAt { get; set; }

    public int Attempts { get; set; }

    public string? LastError { get; set; }
}
