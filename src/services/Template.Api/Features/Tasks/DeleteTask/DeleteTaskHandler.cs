using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Template.Api.Persistence;

namespace Template.Api.Features.Tasks.DeleteTask;

public static class DeleteTaskHandler
{
    public static async Task<Results<NoContent, NotFound>> Handle(
        Guid id,
        IApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var task = await dbContext.Tasks
            .SingleOrDefaultAsync(task => task.Id == id, cancellationToken);

        if (task is null)
        {
            return TypedResults.NotFound();
        }

        dbContext.Tasks.Remove(task);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}