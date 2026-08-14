using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Template.Api.Configuration;
using Template.Api.Messaging.Outbox;
using Template.ServiceDefaults.Messaging.Kafka;
using Xunit;

namespace Template.Api.UnitTests.Messaging.Outbox;

public sealed class MessagingRegistrationTests
{
    [Fact]
    public void ConfigureMessaging_registers_deterministic_batch_processor()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["ConnectionStrings:ApiDatabase"] =
            "Host=localhost;Database=registration_test;Username=test;Password=test";
        builder.Configuration["Kafka:BootstrapServers"] = "localhost:9092";

        builder.ConfigureMessaging();

        builder.Services.Should().ContainSingle(x =>
            x.ServiceType == typeof(IOutboxBatchProcessor)
            && x.ImplementationType == typeof(OutboxBatchProcessor)
            && x.Lifetime == ServiceLifetime.Scoped);
        builder.Services.Should().ContainSingle(x =>
            x.ServiceType == typeof(IOutboxMessageWriter) && x.Lifetime == ServiceLifetime.Scoped);
        builder.Services.Should().ContainSingle(x =>
            x.ServiceType == typeof(IMessagePublisher) && x.Lifetime == ServiceLifetime.Singleton);
    }
}
