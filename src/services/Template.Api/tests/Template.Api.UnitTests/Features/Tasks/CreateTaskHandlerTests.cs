using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Template.Api.Endpoints.Tasks;
using Template.Api.Features.Tasks.CreateTask;
using Template.Api.Messaging.Kafka;
using Template.Api.Messaging.Outbox;
using Xunit;

namespace Template.Api.UnitTests.Features.Tasks;

public sealed class CreateTaskHandlerTests
{
    [Theory]
    [InlineData(" ", "description", "name")]
    [InlineData(null, "description", "name")]
    public async Task Handle_rejects_missing_name(string? name, string description, string errorKey)
    {
        await using var db = TestApplicationDbContext.Create();
        var outbox = Substitute.For<IOutboxMessageWriter>();

        var result = await CreateTaskHandler.Handle(new CreateTaskRequest(name!, description), db, outbox, TestContext.Current.CancellationToken);

        var problem = result.Result.Should().BeOfType<ValidationProblem>().Subject;
        problem.ProblemDetails.Errors.Should().ContainKey(errorKey);
        db.Tasks.Should().BeEmpty();
        outbox.DidNotReceiveWithAnyArgs().AddMessage(default!, default, default!);
    }

    [Fact]
    public async Task Handle_rejects_values_over_maximum_lengths()
    {
        await using var db = TestApplicationDbContext.Create();
        var result = await CreateTaskHandler.Handle(
            new CreateTaskRequest(new string('n', 201), new string('d', 2001)),
            db,
            Substitute.For<IOutboxMessageWriter>(),
            TestContext.Current.CancellationToken);

        var errors = result.Result.Should().BeOfType<ValidationProblem>().Subject.ProblemDetails.Errors;
        errors.Should().ContainKeys("name", "description");
    }

    [Fact]
    public async Task Handle_trims_and_persists_task_with_snapshot()
    {
        await using var db = TestApplicationDbContext.Create();
        var outbox = Substitute.For<IOutboxMessageWriter>();

        var result = await CreateTaskHandler.Handle(
            new CreateTaskRequest("  Name  ", "  Description  "), db, outbox, TestContext.Current.CancellationToken);

        var created = result.Result.Should().BeOfType<Created<TaskResponse>>().Subject;
        created.Value.Should().Match<TaskResponse>(x => x.Name == "Name" && x.Description == "Description");
        var task = db.Tasks.Should().ContainSingle().Subject;
        outbox.Received(1).AddMessage(TaskKafkaTopics.Tasks, task.Id,
            Arg.Is<TaskSnapshot>(x => x.Id == task.Id && x.Name == "Name" && x.Description == "Description"));
    }
}
