using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Template.Api.Messaging.Kafka;
using Template.Api.Persistence;

namespace Template.Api.Features.Tasks.DeleteTask;

public static class DeleteTaskHandler
{
    public static async Task<Results<NoContent, NotFound>> Handle(
        Guid id,
        IApplicationDbContext dbContext,
        ITaskEventPublisher taskEventPublisher,
        CancellationToken cancellationToken)
    {
        var task = await dbContext.Tasks
            .SingleOrDefaultAsync(task => task.Id == id, cancellationToken);

        if (task is null)
        {
            return TypedResults.NotFound();
        }

        var taskSnapshot = TaskSnapshot.FromEntity(task);

        dbContext.Tasks.Remove(task);
        await dbContext.SaveChangesAsync(cancellationToken);
        await taskEventPublisher.PublishAsync("deleted", taskSnapshot, cancellationToken);

        return TypedResults.NoContent();
    }
}
