using Template.Api.Endpoints.Tasks;
using Template.ServiceDefaults;

namespace Template.Api.Configuration;

public static partial class Configure
{
    public static WebApplication ConfigureApp(this WebApplicationBuilder builder)
    {
        var app = builder
            .ConfigureSwagger()
            .ConfigurePersistence()
            .ConfigureMessaging()
            .ConfigureCors()
            .AddServiceDefaults()
            .Build();

        app.RunMigrations();

        app.MapDefaultEndpoints();
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors(ClientOrigins);

        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapTasksEndpoints();

        return app;
    }
}
