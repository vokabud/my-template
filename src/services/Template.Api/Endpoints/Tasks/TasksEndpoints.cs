using Microsoft.EntityFrameworkCore;
using Template.Api.Domain;
using Template.Api.Persistence;

namespace Template.Api.Endpoints.Tasks;

public static class TasksEndpoints
{
    public static void MapTasksEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/tasks", GetTasks);
        app.MapGet("/tasks/{id:guid}", GetTaskById);
        app.MapPost("/tasks", CreateTask);
        app.MapPut("/tasks/{id:guid}", UpdateTask);
        app.MapDelete("/tasks/{id:guid}", DeleteTask);
    }

    static async Task<IResult> GetTasks(IApplicationDbContext dbContext)
    {
        var tasks = await dbContext
            .Tasks
            .ToListAsync();

        return Results.Ok(tasks);
    }

    static async Task<IResult> GetTaskById(Guid id, IApplicationDbContext dbContext)
    {
        var task = await dbContext.Tasks.FindAsync(id);

        if (task is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(task);
    }

    static async Task<IResult> CreateTask(TaskEntity task, IApplicationDbContext dbContext)
    {
        task.Id = Guid.NewGuid();

        dbContext.Tasks.Add(task);
        await dbContext.SaveChangesAsync();

        return Results.Created($"/tasks/{task.Id}", task);
    }

    static async Task<IResult> UpdateTask(Guid id, TaskEntity input, IApplicationDbContext dbContext)
    {
        var task = await dbContext.Tasks.FindAsync(id);

        if (task is null)
        {
            return Results.NotFound();
        }

        task.Name = input.Name;
        task.Description = input.Description;

        await dbContext.SaveChangesAsync();

        return Results.Ok(task);
    }

    static async Task<IResult> DeleteTask(Guid id, IApplicationDbContext dbContext)
    {
        var task = await dbContext.Tasks.FindAsync(id);

        if (task is null)
        {
            return Results.NotFound();
        }

        dbContext.Tasks.Remove(task);
        await dbContext.SaveChangesAsync();

        return Results.NoContent();
    }
}
