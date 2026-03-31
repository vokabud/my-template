namespace Template.ServiceDefaults.Messaging.Kafka;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; set; } = "localhost:9092";

    public string ClientId { get; set; } = "template-api";
}
