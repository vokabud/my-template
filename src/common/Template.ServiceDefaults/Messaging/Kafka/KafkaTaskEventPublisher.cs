using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Template.ServiceDefaults.Messaging.Kafka;

public sealed class KafkaMessagePublisher : IMessagePublisher, IDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IProducer<string, string?> _producer;
    private readonly ILogger<KafkaMessagePublisher> _logger;

    public KafkaMessagePublisher(
        IOptions<KafkaOptions> options,
        ILogger<KafkaMessagePublisher> logger)
    {
        var kafkaOptions = options.Value;

        _logger = logger;

        if (string.IsNullOrWhiteSpace(kafkaOptions.BootstrapServers))
        {
            throw new InvalidOperationException("Kafka bootstrap servers are not configured.");
        }

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = kafkaOptions.BootstrapServers,
            ClientId = kafkaOptions.ClientId
        };

        _producer = new ProducerBuilder<string, string?>(producerConfig).Build();
    }

    public async Task PublishTombstoneAsync(
        string topic,
        Guid id,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);

        var result = await _producer
            .ProduceAsync(
                topic,
                null,
                cancellationToken);

        _logger.LogInformation(
            "Published Tombstone Kafka message for entity {id} to topic {Topic} at offset {Offset}.",
            id,
            result.Topic,
            result.Offset.Value);
    }

    public async Task PublishAsync(
        string topic,
        Guid id,
        object body,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);

        var message = new Message<string, string?>
        {
            Key = id.ToString(),
            Value = JsonSerializer.Serialize(body, body.GetType(), SerializerOptions)
        };

        var result = await _producer
            .ProduceAsync(
                topic,
                message,
                cancellationToken);

        _logger.LogInformation(
            "Published Kafka message for entity {id} to topic {Topic} at offset {Offset}.",
            id,
            result.Topic,
            result.Offset.Value);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}
