using Microsoft.EntityFrameworkCore;
using Template.BackgroundJobs.Persistence;

namespace Template.BackgroundJobs.Configuration;

public static partial class Configure
{
    public static WebApplicationBuilder ConfigurePersistence(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("BackgroundJobsDatabase")));
        builder.Services.AddScoped<ITaskStore, EfTaskStore>();
        builder.Services.AddSingleton(TimeProvider.System);

        return builder;
    }

    public static WebApplication RunMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        return app;
    }
}
