using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Template.BackgroundJobs.Domain;
using Template.BackgroundJobs.Persistence;
using Xunit;

namespace Template.BackgroundJobs.UnitTests.Persistence;

public sealed class TaskConfigurationTests
{
    [Fact]
    public void Task_model_has_expected_relational_contract()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=model;Username=model;Password=model")
            .Options;
        using var context = new ApplicationDbContext(options);

        var entity = context.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(TaskEntity));

        entity.Should().NotBeNull();
        entity!.GetTableName().Should().Be("Tasks");
        entity.FindPrimaryKey()!.Properties.Select(property => property.Name)
            .Should().Equal(nameof(TaskEntity.Id));
        entity.FindProperty(nameof(TaskEntity.Name))!.GetMaxLength().Should().Be(200);
        entity.FindProperty(nameof(TaskEntity.Status))!.GetTypeMapping().Converter.Should().NotBeNull();
        entity.FindProperty(nameof(TaskEntity.ProcessedAt))!.IsNullable.Should().BeTrue();
        entity.GetIndexes().Should().Contain(index => index.Properties
            .Select(property => property.Name)
            .SequenceEqual(new[] { nameof(TaskEntity.Status), nameof(TaskEntity.Id) }));
        entity.GetCheckConstraints().Select(constraint => constraint.Name).Should().Contain(new[]
        {
            "CK_Tasks_Status",
            "CK_Tasks_Status_ProcessedAt"
        });
    }
}
