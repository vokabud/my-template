namespace Template.BackgroundJobs.Endpoints.Tasks;

public static class TasksEndpoints
{
    public static void MapTasksEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup($"{Configuration.ApiVersioning.V1Prefix}/tasks")
            .WithTags("Tasks")
            .WithGroupName(Configuration.ApiVersioning.V1);

        group.MapGet(string.Empty, GetTasksHandler.Handle)
            .WithName("GetBackgroundJobTasksV1");
    }
}
