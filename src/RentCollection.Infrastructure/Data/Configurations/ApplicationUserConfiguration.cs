using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCollection.Domain.Entities;

namespace RentCollection.Infrastructure.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Viewer");

        builder.Property(u => u.RefreshToken)
            .HasMaxLength(500);

        // Indexes for better query performance
        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
    }
}
