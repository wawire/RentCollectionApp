using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;

namespace RentCollection.Infrastructure.Data.Configurations;

public class MaintenanceRequestConfiguration : IEntityTypeConfiguration<MaintenanceRequest>
{
    public void Configure(EntityTypeBuilder<MaintenanceRequest> builder)
    {
        builder.HasKey(mr => mr.Id);

        builder.Property(mr => mr.EstimatedCost)
            .HasPrecision(18, 2);

        builder.Property(mr => mr.ActualCost)
            .HasPrecision(18, 2);

        builder.Property(mr => mr.Priority)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(mr => mr.Status)
            .IsRequired()
            .HasConversion<int>();

        // Navigation properties
        builder.HasOne(mr => mr.Tenant)
            .WithMany()
            .HasForeignKey(mr => mr.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(mr => mr.Unit)
            .WithMany()
            .HasForeignKey(mr => mr.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(mr => mr.Property)
            .WithMany()
            .HasForeignKey(mr => mr.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(mr => mr.AssignedToUser)
            .WithMany()
            .HasForeignKey(mr => mr.AssignedToUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(mr => mr.TenantId);
        builder.HasIndex(mr => mr.UnitId);
        builder.HasIndex(mr => mr.PropertyId);
        builder.HasIndex(mr => mr.AssignedToUserId);
    }
}
