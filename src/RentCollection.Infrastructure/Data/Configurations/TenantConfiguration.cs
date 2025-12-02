using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCollection.Domain.Entities;

namespace RentCollection.Infrastructure.Data.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(t => t.IdNumber)
            .HasMaxLength(50);

        builder.Property(t => t.MonthlyRent)
            .HasPrecision(18, 2);

        builder.Property(t => t.SecurityDeposit)
            .HasPrecision(18, 2);

        builder.Property(t => t.Notes)
            .HasMaxLength(1000);

        builder.Property(t => t.ApplicationNotes)
            .HasMaxLength(1000);

        // New tenant self-service fields
        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<int>(); // Store enum as int

        // Navigation properties
        builder.HasOne(t => t.User)
            .WithOne(u => u.Tenant)
            .HasForeignKey<Tenant>(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Payments)
            .WithOne(p => p.Tenant)
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.Email);
        builder.HasIndex(t => t.PhoneNumber);
        builder.HasIndex(t => t.Status);
    }
}
