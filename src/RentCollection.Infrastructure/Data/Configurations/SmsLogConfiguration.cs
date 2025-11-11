using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCollection.Domain.Entities;

namespace RentCollection.Infrastructure.Data.Configurations;

public class SmsLogConfiguration : IEntityTypeConfiguration<SmsLog>
{
    public void Configure(EntityTypeBuilder<SmsLog> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(s => s.ExternalId)
            .HasMaxLength(100);

        builder.Property(s => s.ErrorMessage)
            .HasMaxLength(500);

        builder.HasIndex(s => s.PhoneNumber);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.SentAt);
    }
}
