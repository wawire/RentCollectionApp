using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCollection.Domain.Entities;

namespace RentCollection.Infrastructure.Data.Configurations
{
    public class RentReminderConfiguration : IEntityTypeConfiguration<RentReminder>
    {
        public void Configure(EntityTypeBuilder<RentReminder> builder)
        {
            builder.ToTable("RentReminders");

            builder.HasKey(r => r.Id);

            // Decimal precision for currency
            builder.Property(r => r.RentAmount)
                .HasColumnType("decimal(18,2)");

            // Required string fields
            builder.Property(r => r.MessageTemplate)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(r => r.MessageSent)
                .HasMaxLength(1000);

            // Foreign keys with NO ACTION to avoid cascade cycles
            builder.HasOne(r => r.Tenant)
                .WithMany()
                .HasForeignKey(r => r.TenantId)
                .OnDelete(DeleteBehavior.NoAction);  // Avoid cascade cycles

            builder.HasOne(r => r.Landlord)
                .WithMany()
                .HasForeignKey(r => r.LandlordId)
                .OnDelete(DeleteBehavior.NoAction);  // Avoid cascade cycles

            builder.HasOne(r => r.Property)
                .WithMany()
                .HasForeignKey(r => r.PropertyId)
                .OnDelete(DeleteBehavior.NoAction);  // Avoid cascade cycles

            builder.HasOne(r => r.Unit)
                .WithMany()
                .HasForeignKey(r => r.UnitId)
                .OnDelete(DeleteBehavior.NoAction);  // Avoid cascade cycles

            // Indexes for query performance
            builder.HasIndex(r => r.TenantId);
            builder.HasIndex(r => r.LandlordId);
            builder.HasIndex(r => r.ScheduledDate);
            builder.HasIndex(r => r.Status);
            builder.HasIndex(r => new { r.TenantId, r.RentDueDate });
        }
    }
}
