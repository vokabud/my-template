using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.BackgroundJobs.Domain;

namespace Template.BackgroundJobs.Persistence.Configurations;

public sealed class TaskConfiguration : IEntityTypeConfiguration<TaskEntity>
{
    public void Configure(EntityTypeBuilder<TaskEntity> builder)
    {
        builder.ToTable("Tasks", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_Tasks_Status",
                "\"Status\" IN ('Pending', 'Processed')");
            tableBuilder.HasCheckConstraint(
                "CK_Tasks_Status_ProcessedAt",
                "(\"Status\" = 'Pending' AND \"ProcessedAt\" IS NULL) OR " +
                "(\"Status\" = 'Processed' AND \"ProcessedAt\" IS NOT NULL)");
        });

        builder.HasKey(task => task.Id);

        builder.Property(task => task.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(task => task.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(task => new { task.Status, task.Id });
    }
}
