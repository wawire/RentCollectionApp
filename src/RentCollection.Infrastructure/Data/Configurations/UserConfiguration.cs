using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCollection.Domain.Entities;

namespace RentCollection.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for User entity
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(u => u.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // Relationships
        builder.HasOne(u => u.Property)
            .WithMany()
            .HasForeignKey(u => u.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Tenant)
            .WithMany()
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for performance
        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => u.PhoneNumber);

        builder.HasIndex(u => u.Role);

        builder.HasIndex(u => u.Status);
    }
}
