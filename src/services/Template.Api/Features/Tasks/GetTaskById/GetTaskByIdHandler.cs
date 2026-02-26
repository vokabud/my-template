using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Template.Api.Contracts.Tasks;
using Template.Api.Persistence;

namespace Template.Api.Features.Tasks.GetTaskById;

public static class GetTaskByIdHandler
{
    public static async Task<Results<Ok<TaskResponse>, NotFound>> Handle(
        Guid id,
        IApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var task = await dbContext.Tasks
            .AsNoTracking()
            .Where(task => task.Id == id)
            .Select(task => new TaskResponse(task.Id, task.Name, task.Description))
            .SingleOrDefaultAsync(cancellationToken);

        if (task is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(task);
    }
}