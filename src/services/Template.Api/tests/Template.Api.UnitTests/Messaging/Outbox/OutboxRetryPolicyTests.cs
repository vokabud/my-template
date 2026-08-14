using FluentAssertions;
using Template.Api.Messaging.Outbox;
using Xunit;

namespace Template.Api.UnitTests.Messaging.Outbox;

public sealed class OutboxRetryPolicyTests
{
    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 4)]
    [InlineData(8, 256)]
    [InlineData(9, 256)]
    public void GetDelay_caps_exponential_backoff(int attempts, int expectedSeconds)
    {
        OutboxRetryPolicy.GetDelay(attempts).Should().Be(TimeSpan.FromSeconds(expectedSeconds));
    }

    [Fact]
    public void TruncateError_limits_errors_to_database_column_length()
    {
        OutboxRetryPolicy.TruncateError(new string('x', 4001)).Should().HaveLength(4000);
    }
}
