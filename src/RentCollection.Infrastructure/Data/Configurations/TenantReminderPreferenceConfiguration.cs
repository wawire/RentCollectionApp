using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCollection.Domain.Entities;

namespace RentCollection.Infrastructure.Data.Configurations
{
    public class TenantReminderPreferenceConfiguration : IEntityTypeConfiguration<TenantReminderPreference>
    {
        public void Configure(EntityTypeBuilder<TenantReminderPreference> builder)
        {
            builder.ToTable("TenantReminderPreferences");

            builder.HasKey(p => p.Id);

            // One preference per tenant
            builder.HasIndex(p => p.TenantId)
                .IsUnique();

            // Contact overrides
            builder.Property(p => p.AlternatePhoneNumber)
                .HasMaxLength(20);

            builder.Property(p => p.AlternateEmail)
                .HasMaxLength(255);

            // Foreign key
            builder.HasOne(p => p.Tenant)
                .WithMany()
                .HasForeignKey(p => p.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
