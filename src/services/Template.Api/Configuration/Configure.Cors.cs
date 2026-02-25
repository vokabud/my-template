namespace Template.Api.Configuration;

public static partial class Configure
{
    private const string ClientOrigins = "ClientOrigins";

    public static WebApplicationBuilder ConfigureCors(
        this WebApplicationBuilder builder)
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(ClientOrigins, policy =>
            {
                policy
                    .WithOrigins(allowedOrigins ?? [])
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return builder;
    }
}
