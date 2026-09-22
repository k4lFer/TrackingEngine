using App.Domain.User.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Core.DataBaseContext.Configurations.User;

public class EmailVerificationConfiguration : IEntityTypeConfiguration<TEmailVerification>
{
    public void Configure(EntityTypeBuilder<TEmailVerification> builder)
    {
        builder.ToTable("email_verifications", schema: "user_credential");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasColumnType("varchar")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.TokenHash)
            .HasColumnName("token_hash")
            .HasColumnType("varchar")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.CodeHash)
            .HasColumnName("code_hash")
            .HasColumnType("varchar")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .IsRequired();

        builder.Property(x => x.LinkExpiresAt)
            .HasColumnName("link_expires_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.CodeExpiresAt)
            .HasColumnName("code_expires_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.UsedAt)
            .HasColumnName("used_at")
            .HasColumnType("timestamptz");

        builder.Property(x => x.FailedAttempts)
            .HasColumnName("failed_attempts")
            .IsRequired();

        builder.Property(x => x.IsValid)
            .HasColumnName("is_valid")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.CodeHash);
        builder.HasIndex(x => x.TokenHash);
    }
}