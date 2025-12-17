using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCollection.Domain.Entities;

namespace RentCollection.Infrastructure.Data.Configurations
{
    public class ReminderSettingsConfiguration : IEntityTypeConfiguration<ReminderSettings>
    {
        public void Configure(EntityTypeBuilder<ReminderSettings> builder)
        {
            builder.ToTable("ReminderSettings");

            builder.HasKey(s => s.Id);

            // One settings per landlord
            builder.HasIndex(s => s.LandlordId)
                .IsUnique();

            // Message templates
            builder.Property(s => s.SevenDaysBeforeTemplate)
                .HasMaxLength(1000);

            builder.Property(s => s.ThreeDaysBeforeTemplate)
                .HasMaxLength(1000);

            builder.Property(s => s.OneDayBeforeTemplate)
                .HasMaxLength(1000);

            builder.Property(s => s.OnDueDateTemplate)
                .HasMaxLength(1000);

            builder.Property(s => s.OneDayOverdueTemplate)
                .HasMaxLength(1000);

            builder.Property(s => s.ThreeDaysOverdueTemplate)
                .HasMaxLength(1000);

            builder.Property(s => s.SevenDaysOverdueTemplate)
                .HasMaxLength(1000);

            // Foreign key
            builder.HasOne(s => s.Landlord)
                .WithMany()
                .HasForeignKey(s => s.LandlordId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
