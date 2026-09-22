using App.Domain.User.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Core.DataBaseContext.Configurations.User;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<TRefreshToken>
{
    public void Configure(EntityTypeBuilder<TRefreshToken> builder)
    {
        builder.ToTable("refresh_tokens", schema: "user_credential");

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
        
        builder.Property(x => x.DeviceId)
            .HasColumnName("device_id")
            .HasColumnType("varchar")
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(x => x.TokenHash)
            .HasColumnName("token_hash")
            .HasColumnType("varchar")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.IpAddress)
            .HasColumnName("ip_address")
            .HasColumnType("varchar")
            .HasMaxLength(45);

        builder.Property(x => x.UserAgent)
            .HasColumnName("user_agent")
            .HasColumnType("varchar")
            .HasMaxLength(255);

        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamptz");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.LastActivityAt)
            .HasColumnName("last_activity_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.RevokedAt)
            .HasColumnName("revoked_at")
            .HasColumnType("timestamptz"); 

        builder.HasOne(x => x.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
    }
}