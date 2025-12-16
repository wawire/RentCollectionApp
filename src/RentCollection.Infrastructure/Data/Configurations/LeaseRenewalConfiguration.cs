using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;

namespace RentCollection.Infrastructure.Data.Configurations;

public class LeaseRenewalConfiguration : IEntityTypeConfiguration<LeaseRenewal>
{
    public void Configure(EntityTypeBuilder<LeaseRenewal> builder)
    {
        builder.HasKey(lr => lr.Id);

        builder.Property(lr => lr.CurrentRentAmount)
            .HasPrecision(18, 2);

        builder.Property(lr => lr.ProposedRentAmount)
            .HasPrecision(18, 2);

        builder.Property(lr => lr.RentIncreasePercentage)
            .HasPrecision(18, 2);

        builder.Property(lr => lr.Status)
            .IsRequired()
            .HasConversion<int>();

        // Navigation properties
        builder.HasOne(lr => lr.Tenant)
            .WithMany()
            .HasForeignKey(lr => lr.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(lr => lr.Unit)
            .WithMany()
            .HasForeignKey(lr => lr.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(lr => lr.Property)
            .WithMany()
            .HasForeignKey(lr => lr.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(lr => lr.TenantId);
        builder.HasIndex(lr => lr.UnitId);
        builder.HasIndex(lr => lr.PropertyId);
    }
}
