using Microsoft.EntityFrameworkCore;
using Template.Api.Persistence;

namespace Template.Api.Endpoints.Tasks;

public static class TasksEndpoints
{
    public static void MapTasksEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/tasks", GetDevices);
    }

    static async Task<IResult> GetDevices(IApplicationDbContext dbContext)
    {
        var tasks = await dbContext
            .Tasks
            .ToListAsync();

        return Results.Ok(tasks);
    }
}