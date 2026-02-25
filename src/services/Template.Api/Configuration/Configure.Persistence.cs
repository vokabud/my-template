using Microsoft.EntityFrameworkCore;
using Template.Api.Persistence;

namespace Template.Api.Configuration;

public static partial class Configure
{
    public static WebApplicationBuilder ConfigurePersistence(
        this WebApplicationBuilder builder)
    {
        builder
            .Services
            .AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

        return builder;
    }

    public static WebApplication RunMigrations(
        this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();

        context.Database.Migrate();

        return app;
    }
}
