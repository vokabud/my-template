using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Template.Api.Domain;
using Template.Api.Endpoints.Tasks;
using Template.Api.IntegrationTests.Infrastructure;
using Template.Api.Messaging.Kafka;
using Template.Api.Messaging.Outbox;
using Template.Api.Persistence;
using Xunit;

namespace Template.Api.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class PostgreSqlIntegrationTests(PostgreSqlFixture fixture) : IAsyncLifetime
{
    public async ValueTask InitializeAsync()
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE \"OutboxMessages\", \"Tasks\";", TestContext.Current.CancellationToken);
        fixture.Publisher.Clear();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Starting_host_applies_all_migrations()
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var pending = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
            .Database.GetPendingMigrationsAsync(TestContext.Current.CancellationToken);
        pending.Should().BeEmpty();
    }

    [Fact]
    public async Task Crud_endpoints_persist_tasks_and_outbox_contracts()
    {
        var client = fixture.Factory.CreateClient();
        var create = await client.PostAsJsonAsync("/api/v1/tasks", new CreateTaskRequest(" Name ", " Description "),
            TestContext.Current.CancellationToken);
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await create.Content.ReadFromJsonAsync<TaskResponse>(TestContext.Current.CancellationToken);
        created.Should().NotBeNull();

        var get = await client.GetFromJsonAsync<TaskResponse>($"/api/v1/tasks/{created!.Id}",
            TestContext.Current.CancellationToken);
        get.Should().BeEquivalentTo(new TaskResponse(created.Id, "Name", "Description"));

        var update = await client.PutAsJsonAsync($"/api/v1/tasks/{created.Id}",
            new UpdateTaskRequest("Updated", "Changed"), TestContext.Current.CancellationToken);
        update.StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.DeleteAsync($"/api/v1/tasks/{created.Id}", TestContext.Current.CancellationToken))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var rows = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().OutboxMessages
            .OrderBy(x => x.CreatedAt).ToListAsync(TestContext.Current.CancellationToken);
        rows.Should().HaveCount(3);
        rows[0].Topic.Should().Be(TaskKafkaTopics.Tasks);
        JsonSerializer.Deserialize<TaskSnapshot>(rows[0].Payload!, new JsonSerializerOptions(JsonSerializerDefaults.Web))!
            .Name.Should().Be("Name");
        rows[2].Payload.Should().BeNull();
    }

    [Fact]
    public async Task Validation_and_missing_resources_return_contract_statuses()
    {
        var client = fixture.Factory.CreateClient();
        (await client.PostAsJsonAsync("/api/v1/tasks", new CreateTaskRequest(" ", "Description"),
            TestContext.Current.CancellationToken)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await client.GetAsync($"/api/v1/tasks/{Guid.NewGuid()}", TestContext.Current.CancellationToken))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Failed_outbox_insert_rolls_back_task_mutation()
    {
        await using (var setupScope = fixture.Factory.Services.CreateAsyncScope())
        {
            var db = setupScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.ExecuteSqlRawAsync("""
                CREATE OR REPLACE FUNCTION fail_outbox_insert() RETURNS trigger AS $$
                BEGIN RAISE EXCEPTION 'forced outbox failure'; END;
                $$ LANGUAGE plpgsql;
                CREATE TRIGGER fail_outbox BEFORE INSERT ON "OutboxMessages"
                FOR EACH ROW EXECUTE FUNCTION fail_outbox_insert();
                """, TestContext.Current.CancellationToken);
        }

        try
        {
            var response = await fixture.Factory.CreateClient().PostAsJsonAsync(
                "/api/v1/tasks", new CreateTaskRequest("Atomic", "Failure"),
                TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

            await using var assertionScope = fixture.Factory.Services.CreateAsyncScope();
            var db = assertionScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            (await db.Tasks.CountAsync(TestContext.Current.CancellationToken)).Should().Be(0);
            (await db.OutboxMessages.CountAsync(TestContext.Current.CancellationToken)).Should().Be(0);
        }
        finally
        {
            await using var cleanupScope = fixture.Factory.Services.CreateAsyncScope();
            await cleanupScope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.ExecuteSqlRawAsync(
                "DROP TRIGGER IF EXISTS fail_outbox ON \"OutboxMessages\"; DROP FUNCTION IF EXISTS fail_outbox_insert();",
                TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task Outbox_batch_publishes_snapshot_and_tombstone_and_marks_success()
    {
        var id = Guid.NewGuid();
        await SeedAsync(
            new OutboxMessage { Id = Guid.NewGuid(), Topic = "tasks.data", Key = id,
                Payload = "{\"id\":\"" + id + "\",\"name\":\"Name\",\"description\":\"Description\"}", CreatedAt = DateTime.UtcNow },
            new OutboxMessage { Id = Guid.NewGuid(), Topic = "tasks.data", Key = Guid.NewGuid(), CreatedAt = DateTime.UtcNow.AddSeconds(1) });

        await ProcessBatchAsync();

        fixture.Publisher.Messages.Should().HaveCount(2);
        fixture.Publisher.Messages.Should().Contain(x => x.Key == id && !x.IsTombstone);
        fixture.Publisher.Messages.Should().Contain(x => x.IsTombstone);
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        (await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().OutboxMessages.ToListAsync(
            TestContext.Current.CancellationToken)).Should().OnlyContain(x => x.ProcessedAt != null);
    }

    [Fact]
    public async Task Outbox_batch_records_retry_and_ignores_ineligible_rows()
    {
        fixture.Publisher.Failure = new InvalidOperationException(new string('x', 4001));
        var eligible = new OutboxMessage { Id = Guid.NewGuid(), Topic = "tasks.data", Key = Guid.NewGuid(),
            Payload = "{}", CreatedAt = DateTime.UtcNow };
        await SeedAsync(eligible,
            new OutboxMessage { Id = Guid.NewGuid(), Topic = "tasks.data", Key = Guid.NewGuid(), Payload = "{}",
                CreatedAt = DateTime.UtcNow, NextAttemptAt = DateTime.UtcNow.AddHours(1) },
            new OutboxMessage { Id = Guid.NewGuid(), Topic = "tasks.data", Key = Guid.NewGuid(), Payload = "{}",
                CreatedAt = DateTime.UtcNow, ProcessedAt = DateTime.UtcNow });
        var before = DateTime.UtcNow;

        await ProcessBatchAsync();

        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var row = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().OutboxMessages
            .SingleAsync(x => x.Id == eligible.Id, TestContext.Current.CancellationToken);
        row.Attempts.Should().Be(1);
        row.LastError.Should().HaveLength(4000);
        row.NextAttemptAt.Should().BeOnOrAfter(before.AddSeconds(2));
    }

    [Fact]
    public async Task Outbox_batch_processes_only_the_oldest_twenty_rows()
    {
        var start = DateTime.UtcNow.AddMinutes(-1);
        await SeedAsync(Enumerable.Range(0, 21).Select(index => new OutboxMessage
        {
            Id = Guid.NewGuid(), Topic = "tasks.data", Key = Guid.NewGuid(), Payload = "{}",
            CreatedAt = start.AddSeconds(index)
        }).ToArray());

        await ProcessBatchAsync();

        fixture.Publisher.Messages.Should().HaveCount(20);
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var pending = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().OutboxMessages
            .Where(x => x.ProcessedAt == null).SingleAsync(TestContext.Current.CancellationToken);
        pending.CreatedAt.Should().BeCloseTo(start.AddSeconds(20), TimeSpan.FromMicroseconds(1));
    }

    private async Task SeedAsync(params OutboxMessage[] rows)
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.OutboxMessages.AddRange(rows);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task ProcessBatchAsync()
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IOutboxBatchProcessor>()
            .ProcessPendingMessagesAsync(TestContext.Current.CancellationToken);
    }
}
