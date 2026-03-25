using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace Template.Api.Messaging.Kafka;

public sealed class KafkaTaskEventPublisher : ITaskEventPublisher, IDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IProducer<string, string> _producer;
    private readonly KafkaOptions _options;
    private readonly ILogger<KafkaTaskEventPublisher> _logger;

    public KafkaTaskEventPublisher(
        IOptions<KafkaOptions> options,
        ILogger<KafkaTaskEventPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.BootstrapServers))
        {
            throw new InvalidOperationException("Kafka bootstrap servers are not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.Topic))
        {
            throw new InvalidOperationException("Kafka topic is not configured.");
        }

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            ClientId = _options.ClientId
        };

        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
    }

    public async Task PublishAsync(string eventType, TaskSnapshot task, CancellationToken cancellationToken)
    {
        var payload = new TaskChangedEvent(
            eventType,
            DateTime.UtcNow,
            task);

        var message = new Message<string, string>
        {
            Key = task.Id.ToString(),
            Value = JsonSerializer.Serialize(payload, SerializerOptions)
        };

        var result = await _producer.ProduceAsync(_options.Topic, message, cancellationToken);

        _logger.LogInformation(
            "Published task event {EventType} for task {TaskId} to topic {Topic} at offset {Offset}.",
            eventType,
            task.Id,
            result.Topic,
            result.Offset.Value);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}
