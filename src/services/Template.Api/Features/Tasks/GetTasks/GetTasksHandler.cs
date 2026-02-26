using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Template.Api.Contracts.Tasks;
using Template.Api.Persistence;

namespace Template.Api.Features.Tasks.GetTasks;

public static class GetTasksHandler
{
    public static async Task<Ok<List<TaskResponse>>> Handle(
        IApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var tasks = await dbContext.Tasks
            .AsNoTracking()
            .Select(task => new TaskResponse(task.Id, task.Name, task.Description))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(tasks);
    }
}