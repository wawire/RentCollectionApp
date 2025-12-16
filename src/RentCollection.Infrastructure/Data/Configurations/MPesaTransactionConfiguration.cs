using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;

namespace RentCollection.Infrastructure.Data.Configurations;

public class MPesaTransactionConfiguration : IEntityTypeConfiguration<MPesaTransaction>
{
    public void Configure(EntityTypeBuilder<MPesaTransaction> builder)
    {
        builder.HasKey(mt => mt.Id);

        builder.Property(mt => mt.Amount)
            .HasPrecision(18, 2);

        builder.Property(mt => mt.Status)
            .IsRequired()
            .HasConversion<int>();

        // Navigation properties
        builder.HasOne(mt => mt.Tenant)
            .WithMany()
            .HasForeignKey(mt => mt.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(mt => mt.Payment)
            .WithMany()
            .HasForeignKey(mt => mt.PaymentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(mt => mt.TenantId);
        builder.HasIndex(mt => mt.PaymentId);
    }
}
