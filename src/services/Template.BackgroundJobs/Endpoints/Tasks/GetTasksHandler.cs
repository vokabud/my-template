using Microsoft.AspNetCore.Http.HttpResults;
using Template.BackgroundJobs.Persistence;

namespace Template.BackgroundJobs.Endpoints.Tasks;

public static class GetTasksHandler
{
    public static async Task<Ok<TaskResponse[]>> Handle(
        ITaskStore taskStore,
        CancellationToken cancellationToken)
    {
        var tasks = await taskStore.GetAllAsync(cancellationToken);
        var response = tasks
            .Select(task => new TaskResponse(
                task.Id,
                task.Name,
                task.Status,
                task.ProcessedAt))
            .ToArray();

        return TypedResults.Ok(response);
    }
}
