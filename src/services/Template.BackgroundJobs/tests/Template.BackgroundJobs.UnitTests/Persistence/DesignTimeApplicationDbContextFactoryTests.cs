using FluentAssertions;
using Template.BackgroundJobs.Persistence;
using Xunit;

namespace Template.BackgroundJobs.UnitTests.Persistence;

public sealed class DesignTimeApplicationDbContextFactoryTests
{
    [Fact]
    public void CreateDbContext_builds_the_Npgsql_model_without_starting_the_host()
    {
        var factory = new DesignTimeApplicationDbContextFactory();

        using var context = factory.CreateDbContext([]);

        context.Database.ProviderName.Should().Be("Npgsql.EntityFrameworkCore.PostgreSQL");
    }
}
