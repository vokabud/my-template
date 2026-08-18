using Testcontainers.PostgreSql;
using Xunit;

namespace Template.BackgroundJobs.IntegrationTests.Infrastructure;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;

    internal string ConnectionString => _container!.GetConnectionString();
    internal RecordingTaskProcessingLogger Logger { get; } = new();
    internal BackgroundJobsFactory Factory { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        try
        {
            _container = new PostgreSqlBuilder("postgres:18-alpine")
                .WithDatabase("template_background_jobs_tests")
                .WithUsername("postgres")
                .WithPassword("postgres-test-password")
                .Build();
            await _container.StartAsync();
            Factory = new BackgroundJobsFactory(ConnectionString, "Development", Logger);
            Factory.CreateClient();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Integration tests require a running Docker-compatible engine.", ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Factory is not null)
            await Factory.DisposeAsync();
        if (_container is not null)
            await _container.DisposeAsync();
    }
}
