namespace Template.Api.Configuration;

public static partial class Configure
{
    public static WebApplicationBuilder ConfigureSwagger(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        return builder;
    }
}
