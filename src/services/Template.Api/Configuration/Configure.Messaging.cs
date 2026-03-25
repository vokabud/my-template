using Confluent.Kafka;
using Template.Api.Messaging.Kafka;

namespace Template.Api.Configuration;

public static partial class Configure
{
    public static WebApplicationBuilder ConfigureMessaging(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddOptions<KafkaOptions>()
            .Bind(builder.Configuration.GetSection(KafkaOptions.SectionName));

        builder.Services.PostConfigure<KafkaOptions>(options =>
        {
            var connectionString = builder.Configuration.GetConnectionString("kafka");
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                options.BootstrapServers = connectionString;
            }
        });

        builder.Services.AddSingleton<ITaskEventPublisher, KafkaTaskEventPublisher>();

        return builder;
    }
}
