using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Template.BackgroundJobs.Domain;
using Template.BackgroundJobs.Endpoints.Tasks;
using Template.BackgroundJobs.IntegrationTests.Infrastructure;
using Template.BackgroundJobs.Jobs;
using Template.BackgroundJobs.Persistence;
using Xunit;
using BackgroundTaskStatus = Template.BackgroundJobs.Domain.TaskStatus;

namespace Template.BackgroundJobs.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class PostgreSqlIntegrationTests(PostgreSqlFixture fixture) : IAsyncLifetime
{
    public async ValueTask InitializeAsync()
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
            .Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE \"Tasks\";",
                TestContext.Current.CancellationToken);
        fixture.Logger.Clear();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Startup_creates_application_and_Hangfire_schemas()
    {
        await using var connection = new NpgsqlConnection(fixture.ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        await using var command = new NpgsqlCommand("""
            SELECT
                to_regclass('public."Tasks"') IS NOT NULL,
                EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'hangfire');
            """, connection);
        await using var reader = await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);
        (await reader.ReadAsync(TestContext.Current.CancellationToken)).Should().BeTrue();
        reader.GetBoolean(0).Should().BeTrue();
        reader.GetBoolean(1).Should().BeTrue();
    }

    [Fact]
    public async Task Database_rejects_invalid_task_states()
    {
        await using var connection = new NpgsqlConnection(fixture.ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        var invalidStatus = async () => await ExecuteInsertAsync(
            connection, Guid.NewGuid(), "Invalid", "Unknown", null);
        var inconsistentState = async () => await ExecuteInsertAsync(
            connection, Guid.NewGuid(), "Inconsistent", "Processed", null);

        await invalidStatus.Should().ThrowAsync<PostgresException>();
        await inconsistentState.Should().ThrowAsync<PostgresException>();
    }

    [Fact]
    public async Task Get_tasks_returns_manually_inserted_rows_in_stable_order()
    {
        var betaId = Guid.NewGuid();
        var alphaId = Guid.NewGuid();
        await using (var connection = new NpgsqlConnection(fixture.ConnectionString))
        {
            await connection.OpenAsync(TestContext.Current.CancellationToken);
            await ExecuteInsertAsync(connection, betaId, "Beta", "Pending", null);
            await ExecuteInsertAsync(connection, alphaId, "Alpha", "Pending", null);
        }

        var response = await fixture.Factory.CreateClient().GetAsync(
            "/api/v1/tasks",
            TestContext.Current.CancellationToken);
        var tasks = await response.Content.ReadFromJsonAsync<TaskResponse[]>(
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        tasks.Should().NotBeNull();
        tasks!.Select(task => task.Name).Should().Equal("Alpha", "Beta");
        tasks.Should().OnlyContain(task => task.Status == BackgroundTaskStatus.Pending);
    }

    [Fact]
    public async Task Competing_processing_jobs_transition_and_log_only_once()
    {
        var id = Guid.NewGuid();
        await using (var connection = new NpgsqlConnection(fixture.ConnectionString))
        {
            await connection.OpenAsync(TestContext.Current.CancellationToken);
            await ExecuteInsertAsync(connection, id, "Import invoices", "Pending", null);
        }

        await using var firstScope = fixture.Factory.Services.CreateAsyncScope();
        await using var secondScope = fixture.Factory.Services.CreateAsyncScope();
        await Task.WhenAll(
            firstScope.ServiceProvider.GetRequiredService<TaskProcessingJob>()
                .ExecuteAsync(id, TestContext.Current.CancellationToken),
            secondScope.ServiceProvider.GetRequiredService<TaskProcessingJob>()
                .ExecuteAsync(id, TestContext.Current.CancellationToken));

        await using var assertionScope = fixture.Factory.Services.CreateAsyncScope();
        var task = await assertionScope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
            .Tasks.SingleAsync(row => row.Id == id, TestContext.Current.CancellationToken);
        task.Status.Should().Be(BackgroundTaskStatus.Processed);
        task.ProcessedAt.Should().NotBeNull();
        fixture.Logger.Messages.Should().ContainSingle(message =>
            message.Contains(id.ToString(), StringComparison.Ordinal) &&
            message.Contains("Import invoices", StringComparison.Ordinal));
    }

    private static async Task ExecuteInsertAsync(
        NpgsqlConnection connection,
        Guid id,
        string name,
        string status,
        DateTimeOffset? processedAt)
    {
        await using var command = new NpgsqlCommand("""
            INSERT INTO "Tasks" ("Id", "Name", "Status", "ProcessedAt")
            VALUES (@id, @name, @status, @processedAt);
            """, connection);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("name", name);
        command.Parameters.AddWithValue("status", status);
        command.Parameters.AddWithValue(
            "processedAt",
            NpgsqlTypes.NpgsqlDbType.TimestampTz,
            processedAt is null ? DBNull.Value : processedAt.Value);
        await command.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
    }
}
