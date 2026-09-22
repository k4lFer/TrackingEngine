using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Core.DataBaseContext.Audit;

public sealed class AuditMessageConfiguration : IEntityTypeConfiguration<AuditMessage>
{
    public void Configure(EntityTypeBuilder<AuditMessage> builder)
    {
        builder.ToTable("domain_events", schema: "audit");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasColumnType("varchar")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Content)
            .HasColumnName("content")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.OccurredAtUtc)
            .HasColumnName("occurred_at_utc")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.Error)
            .HasColumnName("error")
            .HasColumnType("text");

        builder.HasIndex(x => x.OccurredAtUtc)
            .HasDatabaseName("ix_domain_events_occurred_at_utc");

        builder.HasIndex(x => x.Type)
            .HasDatabaseName("ix_domain_events_type");
    }
}
