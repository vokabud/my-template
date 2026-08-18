using System.Net;
using FluentAssertions;
using Template.BackgroundJobs.IntegrationTests.Infrastructure;
using Xunit;

namespace Template.BackgroundJobs.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class DashboardEnvironmentTests(PostgreSqlFixture fixture)
{
    [Fact]
    public async Task Dashboard_is_available_anonymously_only_in_Development()
    {
        using var developmentClient = fixture.Factory.CreateClient();
        var developmentResponse = await developmentClient.GetAsync(
            "/hangfire/",
            TestContext.Current.CancellationToken);

        await using var productionFactory = new BackgroundJobsFactory(
            fixture.ConnectionString,
            "Production",
            new RecordingTaskProcessingLogger());
        using var productionClient = productionFactory.CreateClient();
        var productionResponse = await productionClient.GetAsync(
            "/hangfire/",
            TestContext.Current.CancellationToken);

        developmentResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        productionResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
