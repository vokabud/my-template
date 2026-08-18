using Xunit;

namespace Template.BackgroundJobs.IntegrationTests.Infrastructure;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class IntegrationTestCollection : ICollectionFixture<PostgreSqlFixture>
{
    public const string Name = "Background jobs PostgreSQL integration tests";
}
