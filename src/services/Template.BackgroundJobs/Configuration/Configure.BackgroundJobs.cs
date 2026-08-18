using Hangfire;
using Hangfire.PostgreSql;
using Template.BackgroundJobs.Jobs;

namespace Template.BackgroundJobs.Configuration;

public static partial class Configure
{
    public static WebApplicationBuilder ConfigureBackgroundJobs(
        this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("BackgroundJobsDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'BackgroundJobsDatabase' is required.");

        builder.Services
            .AddOptions<BackgroundJobOptions>()
            .Bind(builder.Configuration.GetSection(BackgroundJobOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddHangfire(configuration => configuration
            .UsePostgreSqlStorage(
                options => options.UseNpgsqlConnection(connectionString),
                new PostgreSqlStorageOptions
                {
                    SchemaName = "hangfire",
                    PrepareSchemaIfNecessary = true
                }));
        builder.Services.AddHangfireServer();

        builder.Services.AddScoped<TaskDispatcherJob>();
        builder.Services.AddScoped<TaskProcessingJob>();
        builder.Services.AddSingleton<ITaskJobEnqueuer, HangfireTaskJobEnqueuer>();

        return builder;
    }

    public static WebApplication RegisterRecurringJobs(this WebApplication app)
    {
        var manager = app.Services.GetRequiredService<IRecurringJobManager>();
        manager.AddOrUpdate<TaskDispatcherJob>(
            "dispatch-pending-tasks",
            job => job.ExecuteAsync(CancellationToken.None),
            Cron.Minutely,
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

        return app;
    }
}
