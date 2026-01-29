using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCollection.Domain.Entities;

namespace RentCollection.Infrastructure.Data.Configurations
{
    public class MoveOutInspectionConfiguration : IEntityTypeConfiguration<MoveOutInspection>
    {
        public void Configure(EntityTypeBuilder<MoveOutInspection> builder)
        {
            builder.ToTable("MoveOutInspections");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.OverallCondition)
                .HasMaxLength(50);

            builder.Property(m => m.GeneralNotes)
                .HasMaxLength(2000);

            builder.Property(m => m.SettlementNotes)
                .HasMaxLength(1000);

            builder.Property(m => m.RefundMethod)
                .HasMaxLength(50);

            builder.Property(m => m.RefundReference)
                .HasMaxLength(100);

            // Decimal precision
            builder.Property(m => m.CleaningCharges).HasColumnType("decimal(18,2)");
            builder.Property(m => m.RepairCharges).HasColumnType("decimal(18,2)");
            builder.Property(m => m.UnpaidRent).HasColumnType("decimal(18,2)");
            builder.Property(m => m.UnpaidUtilities).HasColumnType("decimal(18,2)");
            builder.Property(m => m.OtherCharges).HasColumnType("decimal(18,2)");
            builder.Property(m => m.TotalDeductions).HasColumnType("decimal(18,2)");
            builder.Property(m => m.SecurityDepositHeld).HasColumnType("decimal(18,2)");
            builder.Property(m => m.RefundAmount).HasColumnType("decimal(18,2)");
            builder.Property(m => m.TenantOwes).HasColumnType("decimal(18,2)");

            // Relationships
            builder.HasOne(m => m.Tenant)
                .WithMany()
                .HasForeignKey(m => m.TenantId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(m => m.Unit)
                .WithMany()
                .HasForeignKey(m => m.UnitId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(m => m.Property)
                .WithMany()
                .HasForeignKey(m => m.PropertyId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(m => m.InspectedBy)
                .WithMany()
                .HasForeignKey(m => m.InspectedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Indexes
            builder.HasIndex(m => m.TenantId);
            builder.HasIndex(m => m.UnitId);
            builder.HasIndex(m => m.PropertyId);
            builder.HasIndex(m => m.Status);
            builder.HasIndex(m => m.MoveOutDate);
        }
    }

    public class InspectionItemConfiguration : IEntityTypeConfiguration<InspectionItem>
    {
        public void Configure(EntityTypeBuilder<InspectionItem> builder)
        {
            builder.ToTable("InspectionItems");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.ItemName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(i => i.MoveInCondition)
                .HasMaxLength(500);

            builder.Property(i => i.MoveOutCondition)
                .HasMaxLength(500);

            builder.Property(i => i.DamageDescription)
                .HasMaxLength(1000);

            builder.Property(i => i.Notes)
                .HasMaxLength(1000);

            builder.Property(i => i.EstimatedRepairCost).HasColumnType("decimal(18,2)");

            // Relationship
            builder.HasOne(i => i.MoveOutInspection)
                .WithMany(m => m.InspectionItems)
                .HasForeignKey(i => i.MoveOutInspectionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index
            builder.HasIndex(i => i.MoveOutInspectionId);
            builder.HasIndex(i => i.Category);
        }
    }

    public class InspectionPhotoConfiguration : IEntityTypeConfiguration<InspectionPhoto>
    {
        public void Configure(EntityTypeBuilder<InspectionPhoto> builder)
        {
            builder.ToTable("InspectionPhotos");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.PhotoUrl)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(p => p.Caption)
                .HasMaxLength(500);

            // Relationships
            builder.HasOne(p => p.MoveOutInspection)
                .WithMany(m => m.Photos)
                .HasForeignKey(p => p.MoveOutInspectionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.InspectionItem)
                .WithMany(i => i.Photos)
                .HasForeignKey(p => p.InspectionItemId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            // Indexes
            builder.HasIndex(p => p.MoveOutInspectionId);
            builder.HasIndex(p => p.InspectionItemId);
            builder.HasIndex(p => p.PhotoType);
        }
    }
}
