using Microsoft.AspNetCore.Http.HttpResults;
using Template.Api.Contracts.Tasks;
using Template.Api.Domain;
using Template.Api.Persistence;

namespace Template.Api.Features.Tasks.CreateTask;

public static class CreateTaskHandler
{
    public static async Task<Results<Created<TaskResponse>, ValidationProblem>> Handle(
        CreateTaskRequest request,
        IApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var errors = Validate(request.Name, request.Description);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = (request.Description ?? string.Empty).Trim()
        };

        dbContext.Tasks.Add(task);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new TaskResponse(task.Id, task.Name, task.Description);

        return TypedResults.Created($"/tasks/{task.Id}", response);
    }

    private static Dictionary<string, string[]> Validate(string? name, string? description)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(name))
        {
            errors["name"] = ["Name is required."];
        }
        else if (name.Length > 200)
        {
            errors["name"] = ["Name must be at most 200 characters."];
        }

        if (!string.IsNullOrEmpty(description) && description.Length > 2000)
        {
            errors["description"] = ["Description must be at most 2000 characters."];
        }

        return errors;
    }
}