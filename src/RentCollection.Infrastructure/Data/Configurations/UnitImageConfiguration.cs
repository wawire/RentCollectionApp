using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCollection.Domain.Entities;

namespace RentCollection.Infrastructure.Data.Configurations;

public class UnitImageConfiguration : IEntityTypeConfiguration<UnitImage>
{
    public void Configure(EntityTypeBuilder<UnitImage> builder)
    {
        builder.HasKey(ui => ui.Id);

        builder.Property(ui => ui.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ui => ui.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(ui => ui.IsPrimary)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationship with Unit
        builder.HasOne(ui => ui.Unit)
            .WithMany(u => u.Images)
            .HasForeignKey(ui => ui.UnitId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for faster queries
        builder.HasIndex(ui => ui.UnitId);
        builder.HasIndex(ui => new { ui.UnitId, ui.IsPrimary });
    }
}
