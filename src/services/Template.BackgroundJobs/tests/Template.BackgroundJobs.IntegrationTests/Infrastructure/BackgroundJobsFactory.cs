using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Template.BackgroundJobs.Jobs;

namespace Template.BackgroundJobs.IntegrationTests.Infrastructure;

internal sealed class BackgroundJobsFactory(
    string connectionString,
    string environment,
    RecordingTaskProcessingLogger logger) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(environment);
        builder.UseSetting("ConnectionStrings:BackgroundJobsDatabase", connectionString);
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<Microsoft.Extensions.Logging.ILogger<TaskProcessingJob>>(logger);
        });
    }
}
