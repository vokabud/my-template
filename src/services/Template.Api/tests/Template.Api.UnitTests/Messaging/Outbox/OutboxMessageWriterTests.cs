using System.Text.Json;
using FluentAssertions;
using Template.Api.Messaging.Kafka;
using Template.Api.Messaging.Outbox;
using Xunit;

namespace Template.Api.UnitTests.Messaging.Outbox;

public sealed class OutboxMessageWriterTests
{
    [Fact]
    public void AddMessage_serializes_web_json_and_adds_row()
    {
        using var db = TestApplicationDbContext.Create();
        var id = Guid.NewGuid();
        new OutboxMessageWriter(db).AddMessage("tasks.data", id, new TaskSnapshot(id, "Name", "Description"));

        var row = db.ChangeTracker.Entries<Template.Api.Domain.OutboxMessage>()
            .Should().ContainSingle().Subject.Entity;
        row.Topic.Should().Be("tasks.data");
        row.Key.Should().Be(id);
        using var json = JsonDocument.Parse(row.Payload!);
        json.RootElement.GetProperty("id").GetGuid().Should().Be(id);
        json.RootElement.GetProperty("name").GetString().Should().Be("Name");
    }

    [Fact]
    public void AddTombstone_adds_null_payload()
    {
        using var db = TestApplicationDbContext.Create();
        new OutboxMessageWriter(db).AddTombstone("tasks.data", Guid.NewGuid());
        db.ChangeTracker.Entries<Template.Api.Domain.OutboxMessage>()
            .Should().ContainSingle().Which.Entity.Payload.Should().BeNull();
    }

    [Fact]
    public void Invalid_arguments_are_rejected()
    {
        using var db = TestApplicationDbContext.Create();
        var writer = new OutboxMessageWriter(db);
        FluentActions.Invoking(() => writer.AddMessage(" ", Guid.NewGuid(), new object())).Should().Throw<ArgumentException>();
        FluentActions.Invoking(() => writer.AddMessage("topic", Guid.NewGuid(), null!)).Should().Throw<ArgumentNullException>();
    }
}
