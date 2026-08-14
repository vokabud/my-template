using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Template.Api.Messaging.Outbox;
using Template.ServiceDefaults.Messaging.Kafka;

namespace Template.Api.IntegrationTests.Infrastructure;

internal sealed class ApiFactory(string connectionString, RecordingMessagePublisher publisher)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:ApiDatabase", connectionString);
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IMessagePublisher>();
            var hosted = services.FirstOrDefault(x => x.ServiceType == typeof(IHostedService)
                && x.ImplementationType == typeof(OutboxMessageProcessor));
            if (hosted is not null) services.Remove(hosted);
            services.AddSingleton<IMessagePublisher>(publisher);
        });
    }
}
