using App.Domain.User.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Core.DataBaseContext.Configurations.User;

public class UserGatewayConfiguration : IEntityTypeConfiguration<TUserGateway>
{
    public void Configure(EntityTypeBuilder<TUserGateway> builder)
    {
        builder.ToTable("user_gateways", schema: "user_credential");
        
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

        builder.Property(x => x.Provider)
            .HasColumnName("provider")
            .HasColumnType("varchar")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ProviderId)
            .HasColumnName("provider_id")
            .HasColumnType("varchar")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany(u => u.UserGateways)
            .HasForeignKey(x => x.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }

}