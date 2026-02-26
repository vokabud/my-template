using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Template.Api.Contracts.Tasks;
using Template.Api.Persistence;

namespace Template.Api.Features.Tasks.UpdateTask;

public static class UpdateTaskHandler
{
    public static async Task<Results<Ok<TaskResponse>, NotFound, ValidationProblem>> Handle(
        Guid id,
        UpdateTaskRequest request,
        IApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var errors = Validate(request.Name, request.Description);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var task = await dbContext.Tasks
            .SingleOrDefaultAsync(task => task.Id == id, cancellationToken);

        if (task is null)
        {
            return TypedResults.NotFound();
        }

        task.Name = request.Name.Trim();
        task.Description = (request.Description ?? string.Empty).Trim();

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new TaskResponse(task.Id, task.Name, task.Description);

        return TypedResults.Ok(response);
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