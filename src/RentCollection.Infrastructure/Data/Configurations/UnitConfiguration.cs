using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCollection.Domain.Entities;

namespace RentCollection.Infrastructure.Data.Configurations;

public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.UnitNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.MonthlyRent)
            .HasPrecision(18, 2);

        builder.Property(u => u.SquareFeet)
            .HasPrecision(18, 2);

        builder.Property(u => u.Description)
            .HasMaxLength(500);

        builder.Property(u => u.RentalType)
            .IsRequired()
            .HasConversion<int>();

        builder.HasMany(u => u.Tenants)
            .WithOne(t => t.Unit)
            .HasForeignKey(t => t.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => new { u.PropertyId, u.UnitNumber }).IsUnique();
    }
}
