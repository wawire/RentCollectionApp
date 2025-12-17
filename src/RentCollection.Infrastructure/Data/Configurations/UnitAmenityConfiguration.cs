using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCollection.Domain.Entities;

namespace RentCollection.Infrastructure.Data.Configurations;

public class UnitAmenityConfiguration : IEntityTypeConfiguration<UnitAmenity>
{
    public void Configure(EntityTypeBuilder<UnitAmenity> builder)
    {
        // Composite primary key
        builder.HasKey(ua => new { ua.UnitId, ua.AmenityId });

        // Relationship with Unit
        builder.HasOne(ua => ua.Unit)
            .WithMany(u => u.UnitAmenities)
            .HasForeignKey(ua => ua.UnitId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with Amenity
        builder.HasOne(ua => ua.Amenity)
            .WithMany(a => a.UnitAmenities)
            .HasForeignKey(ua => ua.AmenityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(ua => ua.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
