using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Template.Api.Domain;
using Template.Api.Endpoints.Tasks;
using Template.Api.Features.Tasks.DeleteTask;
using Template.Api.Features.Tasks.GetTaskById;
using Template.Api.Features.Tasks.GetTasks;
using Template.Api.Messaging.Kafka;
using Template.Api.Messaging.Outbox;
using Xunit;

namespace Template.Api.UnitTests.Features.Tasks;

public sealed class DeleteAndQueryTaskHandlerTests
{
    [Fact]
    public async Task Delete_removes_task_and_writes_tombstone()
    {
        await using var db = TestApplicationDbContext.Create();
        var entity = new TaskEntity { Id = Guid.NewGuid(), Name = "Name", Description = "Description" };
        db.Tasks.Add(entity);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var outbox = Substitute.For<IOutboxMessageWriter>();

        var result = await DeleteTaskHandler.Handle(entity.Id, db, outbox, TestContext.Current.CancellationToken);

        result.Result.Should().BeOfType<NoContent>();
        db.Tasks.Should().BeEmpty();
        outbox.Received(1).AddTombstone(TaskKafkaTopics.Tasks, entity.Id);
    }

    [Fact]
    public async Task Delete_returns_not_found_for_unknown_task()
    {
        await using var db = TestApplicationDbContext.Create();
        var result = await DeleteTaskHandler.Handle(Guid.NewGuid(), db, Substitute.For<IOutboxMessageWriter>(), TestContext.Current.CancellationToken);
        result.Result.Should().BeOfType<NotFound>();
    }

    [Fact]
    public async Task Get_by_id_returns_existing_task_and_not_found_for_unknown_task()
    {
        await using var db = TestApplicationDbContext.Create();
        var entity = new TaskEntity { Id = Guid.NewGuid(), Name = "Name", Description = "Description" };
        db.Tasks.Add(entity);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var found = await GetTaskByIdHandler.Handle(entity.Id, db, TestContext.Current.CancellationToken);
        found.Result.Should().BeOfType<Ok<TaskResponse>>().Which.Value.Should().BeEquivalentTo(
            new TaskResponse(entity.Id, "Name", "Description"));
        var missing = await GetTaskByIdHandler.Handle(Guid.NewGuid(), db, TestContext.Current.CancellationToken);
        missing.Result.Should().BeOfType<NotFound>();
    }

    [Fact]
    public async Task Get_all_projects_every_task()
    {
        await using var db = TestApplicationDbContext.Create();
        db.Tasks.AddRange(
            new TaskEntity { Id = Guid.NewGuid(), Name = "One", Description = "First" },
            new TaskEntity { Id = Guid.NewGuid(), Name = "Two", Description = "Second" });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await GetTasksHandler.Handle(db, TestContext.Current.CancellationToken);
        result.Value.Should().BeEquivalentTo(db.Tasks.Select(x => new TaskResponse(x.Id, x.Name, x.Description)));
    }
}
