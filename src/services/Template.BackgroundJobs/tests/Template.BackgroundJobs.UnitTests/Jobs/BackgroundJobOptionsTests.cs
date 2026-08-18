using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Template.BackgroundJobs.Jobs;
using Xunit;

namespace Template.BackgroundJobs.UnitTests.Jobs;

public sealed class BackgroundJobOptionsTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(1, true)]
    [InlineData(100, true)]
    public void Dispatcher_batch_size_must_be_positive(int batchSize, bool expectedValid)
    {
        var options = new BackgroundJobOptions { DispatcherBatchSize = batchSize };

        var valid = Validator.TryValidateObject(
            options,
            new ValidationContext(options),
            [],
            validateAllProperties: true);

        valid.Should().Be(expectedValid);
    }
}
