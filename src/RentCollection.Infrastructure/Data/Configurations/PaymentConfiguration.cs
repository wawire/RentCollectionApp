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

        builder.HasIndex(p => p.PaymentDate);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.TransactionReference);
    }
}
