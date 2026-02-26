using Template.Api.Features.Tasks.CreateTask;
using Template.Api.Features.Tasks.DeleteTask;
using Template.Api.Features.Tasks.GetTaskById;
using Template.Api.Features.Tasks.GetTasks;
using Template.Api.Features.Tasks.UpdateTask;

namespace Template.Api.Endpoints.Tasks;

public static class TasksEndpoints
{
    public static void MapTasksEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup($"{Configuration.ApiVersioning.V1Prefix}/tasks")
            .WithTags("Tasks")
            .WithGroupName(Configuration.ApiVersioning.V1);

        group.MapGet(string.Empty, GetTasksHandler.Handle)
            .WithName("GetTasksV1");

        group.MapGet("/{id:guid}", GetTaskByIdHandler.Handle)
            .WithName("GetTaskByIdV1");

        group.MapPost(string.Empty, CreateTaskHandler.Handle)
            .WithName("CreateTaskV1");

        group.MapPut("/{id:guid}", UpdateTaskHandler.Handle)
            .WithName("UpdateTaskV1");

        group.MapDelete("/{id:guid}", DeleteTaskHandler.Handle)
            .WithName("DeleteTaskV1");
    }
}