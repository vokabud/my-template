using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Template.Api.Domain;
using Template.Api.Endpoints.Tasks;
using Template.Api.Features.Tasks.UpdateTask;
using Template.Api.Messaging.Kafka;
using Template.Api.Messaging.Outbox;
using Xunit;

namespace Template.Api.UnitTests.Features.Tasks;

public sealed class UpdateTaskHandlerTests
{
    [Fact]
    public async Task Handle_returns_not_found_without_outbox_message()
    {
        await using var db = TestApplicationDbContext.Create();
        var outbox = Substitute.For<IOutboxMessageWriter>();
        var result = await UpdateTaskHandler.Handle(Guid.NewGuid(), new("Name", "Description"), db, outbox, TestContext.Current.CancellationToken);
        result.Result.Should().BeOfType<NotFound>();
        outbox.DidNotReceiveWithAnyArgs().AddMessage(default!, default, default!);
    }

    [Fact]
    public async Task Handle_trims_updates_and_writes_snapshot()
    {
        await using var db = TestApplicationDbContext.Create();
        var entity = new TaskEntity { Id = Guid.NewGuid(), Name = "Old", Description = "Old" };
        db.Tasks.Add(entity);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var outbox = Substitute.For<IOutboxMessageWriter>();

        var result = await UpdateTaskHandler.Handle(entity.Id, new(" New ", " Description "), db, outbox, TestContext.Current.CancellationToken);

        result.Result.Should().BeOfType<Ok<TaskResponse>>().Which.Value.Should()
            .Match<TaskResponse>(x => x.Name == "New" && x.Description == "Description");
        outbox.Received(1).AddMessage(TaskKafkaTopics.Tasks, entity.Id,
            Arg.Is<TaskSnapshot>(x => x.Name == "New" && x.Description == "Description"));
    }
}
