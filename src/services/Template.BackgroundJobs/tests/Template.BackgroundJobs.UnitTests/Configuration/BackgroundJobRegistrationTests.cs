using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Template.BackgroundJobs.Configuration;
using Template.BackgroundJobs.Jobs;
using Xunit;

namespace Template.BackgroundJobs.UnitTests.Configuration;

public sealed class BackgroundJobRegistrationTests
{
    [Fact]
    public void ConfigureBackgroundJobs_registers_jobs_and_default_options()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["ConnectionStrings:BackgroundJobsDatabase"] =
            "Host=localhost;Database=jobs;Username=jobs;Password=jobs";

        builder.ConfigureBackgroundJobs();

        builder.Services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(ITaskJobEnqueuer) &&
            descriptor.ImplementationType == typeof(HangfireTaskJobEnqueuer) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
        using var provider = builder.Services.BuildServiceProvider();
        provider.GetRequiredService<IOptions<BackgroundJobOptions>>()
            .Value.DispatcherBatchSize.Should().Be(100);
    }

    [Fact]
    public void ConfigureBackgroundJobs_rejects_non_positive_batch_size()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["ConnectionStrings:BackgroundJobsDatabase"] =
            "Host=localhost;Database=jobs;Username=jobs;Password=jobs";
        builder.Configuration["BackgroundJobs:DispatcherBatchSize"] = "0";
        builder.ConfigureBackgroundJobs();
        using var provider = builder.Services.BuildServiceProvider();

        var readOptions = () => provider.GetRequiredService<IOptions<BackgroundJobOptions>>().Value;

        readOptions.Should().Throw<OptionsValidationException>();
    }
}
