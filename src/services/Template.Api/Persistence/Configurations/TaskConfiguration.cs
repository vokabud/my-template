using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.Api.Domain;

namespace Template.Api.Persistence.Configurations;

public sealed class TaskConfiguration : IEntityTypeConfiguration<TaskEntity>
{
    public void Configure(EntityTypeBuilder<TaskEntity> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(task => task.Id);

        builder.Property(task => task.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(task => task.Description)
            .IsRequired()
            .HasMaxLength(2000);
    }
}