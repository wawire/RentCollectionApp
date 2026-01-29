using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCollection.Domain.Entities;

namespace RentCollection.Infrastructure.Data.Configurations;

public class UnmatchedPaymentConfiguration : IEntityTypeConfiguration<UnmatchedPayment>
{
    public void Configure(EntityTypeBuilder<UnmatchedPayment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TransactionReference)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.AccountReference)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(x => x.BusinessShortCode)
            .HasMaxLength(20);

        builder.Property(x => x.CorrelationId)
            .HasMaxLength(64);

        builder.Property(x => x.RawPayload)
            .IsRequired();

        builder.Property(x => x.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.LandlordId);

        builder.Property(x => x.PropertyId);

        builder.Property(x => x.ResolutionNotes)
            .HasMaxLength(1000);

        builder.HasIndex(x => x.TransactionReference)
            .IsUnique();

        builder.HasIndex(x => x.LandlordId);

        builder.HasIndex(x => x.PropertyId);
    }
}
