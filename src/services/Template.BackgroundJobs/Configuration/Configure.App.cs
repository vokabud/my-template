using Hangfire;
using Template.BackgroundJobs.Endpoints.Tasks;
using Template.ServiceDefaults;

namespace Template.BackgroundJobs.Configuration;

public static partial class Configure
{
    public static WebApplication ConfigureApp(this WebApplicationBuilder builder)
    {
        var app = builder
            .ConfigurePersistence()
            .ConfigureBackgroundJobs()
            .AddServiceDefaults()
            .Build();

        app.RunMigrations();
        app.RegisterRecurringJobs();

        app.MapDefaultEndpoints();
        app.UseHttpsRedirection();
        app.MapTasksEndpoints();

        if (app.Environment.IsDevelopment())
            app.MapHangfireDashboardWithNoAuthorizationFilters("/hangfire");

        return app;
    }
}
