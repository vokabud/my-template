using Template.Api.Endpoints.Tasks;

namespace Template.Api.Configuration;

public static partial class Configure
{
    public static WebApplication ConfigureApp(this WebApplicationBuilder builder)
    {
        var app = builder
            .ConfigureSwagger()
            .ConfigurePersistence()
            .ConfigureCors()
            .Build();

        app.RunMigrations();

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseCors(ClientOrigins);

        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapTasksEndpoints();

        return app;
    }
}
