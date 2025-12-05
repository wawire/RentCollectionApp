using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCollection.Domain.Entities;

namespace RentCollection.Infrastructure.Data.Configurations;

public class AmenityConfiguration : IEntityTypeConfiguration<Amenity>
{
    public void Configure(EntityTypeBuilder<Amenity> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.IconName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(a => a.Name)
            .IsUnique();
    }
}
