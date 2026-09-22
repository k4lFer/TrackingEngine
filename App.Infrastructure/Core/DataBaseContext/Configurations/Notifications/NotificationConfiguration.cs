using App.Domain.Notifications.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Core.DataBaseContext.Configurations.Notifications;

public class NotificationConfiguration : IEntityTypeConfiguration<TNotification>
{
    public void Configure(EntityTypeBuilder<TNotification> builder)
    {
        builder.ToTable("notifications", schema: "notifications");
        
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
        
        builder.Property(x => x.Type)
            .HasColumnName("type")
            .IsRequired();
        
        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasColumnType("varchar")
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(x => x.Content)
            .HasColumnName("content")
            .HasColumnType("text")
            .IsRequired();
        
        builder.Property(x => x.IsRead)
            .HasColumnName("is_read")
            .IsRequired();
        
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();
        
        builder.Property(x => x.SentAt)
            .HasColumnName("sent_at")
            .HasColumnType("timestamptz");
        
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.CreatedAt);
    }
}
