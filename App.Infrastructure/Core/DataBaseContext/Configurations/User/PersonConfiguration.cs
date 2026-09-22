using App.Domain.User.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Core.DataBaseContext.Configurations.User;

public class PersonConfiguration : IEntityTypeConfiguration<TPerson>
{
    public void Configure(EntityTypeBuilder<TPerson> builder)
    {
        builder.ToTable("persons", schema: "user_credential");

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

        builder.Property(x => x.FirstName)
            .HasColumnName("first_name")
            .HasColumnType("varchar")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasColumnName("last_name")
            .HasColumnType("varchar")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.DateOfBirth)
            .HasColumnName("date_of_birth")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz");
        
        builder.HasIndex(x => x.UserId).IsUnique();
    }
}