using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;

namespace RentCollection.Infrastructure.Data.Configurations;

public class SecurityDepositTransactionConfiguration : IEntityTypeConfiguration<SecurityDepositTransaction>
{
    public void Configure(EntityTypeBuilder<SecurityDepositTransaction> builder)
    {
        builder.HasKey(sdt => sdt.Id);

        builder.Property(sdt => sdt.Amount)
            .HasPrecision(18, 2);

        builder.Property(sdt => sdt.TransactionType)
            .IsRequired()
            .HasConversion<int>();

        // Navigation properties
        builder.HasOne(sdt => sdt.Tenant)
            .WithMany()
            .HasForeignKey(sdt => sdt.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sdt => sdt.RelatedPayment)
            .WithMany()
            .HasForeignKey(sdt => sdt.RelatedPaymentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(sdt => sdt.RelatedMaintenanceRequest)
            .WithMany()
            .HasForeignKey(sdt => sdt.RelatedMaintenanceRequestId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(sdt => sdt.CreatedByUser)
            .WithMany()
            .HasForeignKey(sdt => sdt.CreatedByUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(sdt => sdt.TenantId);
        builder.HasIndex(sdt => sdt.RelatedPaymentId);
        builder.HasIndex(sdt => sdt.RelatedMaintenanceRequestId);
        builder.HasIndex(sdt => sdt.CreatedByUserId);
    }
}
