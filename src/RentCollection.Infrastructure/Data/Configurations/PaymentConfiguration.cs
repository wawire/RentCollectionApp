using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;

namespace RentCollection.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .HasPrecision(18, 2);

        builder.Property(p => p.UnallocatedAmount)
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        builder.Property(p => p.LateFeeAmount)
            .HasPrecision(18, 2);

        builder.Property(p => p.PaymentMethod)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.TransactionReference)
            .HasMaxLength(200);

        builder.Property(p => p.Notes)
            .HasMaxLength(500);

        // New payment proof fields
        builder.Property(p => p.PaymentProofUrl)
            .HasMaxLength(500);

        // Navigation property for confirming user
        builder.HasOne(p => p.ConfirmedBy)
            .WithMany()
            .HasForeignKey(p => p.ConfirmedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.PaymentDate);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.TransactionReference)
            .IsUnique()
            .HasFilter("[TransactionReference] IS NOT NULL");
        builder.HasIndex(p => p.ConfirmedAt);
    }
}
