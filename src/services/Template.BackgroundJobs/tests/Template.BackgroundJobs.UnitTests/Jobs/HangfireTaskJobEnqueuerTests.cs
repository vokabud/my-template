using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Template.BackgroundJobs.Jobs;
using Xunit;

namespace Template.BackgroundJobs.UnitTests.Jobs;

public sealed class HangfireTaskJobEnqueuerTests
{
    [Fact]
    public void Enqueue_creates_a_processing_job_for_the_requested_task()
    {
        var client = new RecordingBackgroundJobClient();
        var sut = new HangfireTaskJobEnqueuer(client);
        var taskId = Guid.NewGuid();

        var jobId = sut.Enqueue(taskId);

        jobId.Should().Be("job-1");
        client.CreatedJob.Should().NotBeNull();
        client.CreatedJob!.Type.Should().Be<TaskProcessingJob>();
        client.CreatedJob.Method.Name.Should().Be(nameof(TaskProcessingJob.ExecuteAsync));
        client.CreatedJob.Args[0].Should().Be(taskId);
        client.CreatedState.Should().BeOfType<EnqueuedState>();
    }

    private sealed class RecordingBackgroundJobClient : IBackgroundJobClient
    {
        public Job? CreatedJob { get; private set; }
        public IState? CreatedState { get; private set; }

        public string Create(Job job, IState state)
        {
            CreatedJob = job;
            CreatedState = state;
            return "job-1";
        }

        public bool ChangeState(string jobId, IState state, string expectedState) =>
            throw new NotSupportedException();
    }
}
