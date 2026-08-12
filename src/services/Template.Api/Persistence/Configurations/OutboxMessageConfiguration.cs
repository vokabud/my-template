using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.Api.Domain;

namespace Template.Api.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Topic)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(message => message.Payload)
            .HasColumnType("jsonb");

        builder.Property(message => message.LastError)
            .HasMaxLength(4000);

        builder.HasIndex(message => new
        {
            message.ProcessedAt,
            message.NextAttemptAt,
            message.CreatedAt
        });
    }
}
