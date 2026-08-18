using System.ComponentModel.DataAnnotations;

namespace Template.BackgroundJobs.Jobs;

public sealed class BackgroundJobOptions
{
    public const string SectionName = "BackgroundJobs";

    [Range(1, int.MaxValue)]
    public int DispatcherBatchSize { get; init; } = 100;
}
