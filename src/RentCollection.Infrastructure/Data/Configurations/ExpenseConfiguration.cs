using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCollection.Domain.Entities;

namespace RentCollection.Infrastructure.Data.Configurations
{
    public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
    {
        public void Configure(EntityTypeBuilder<Expense> builder)
        {
            builder.ToTable("Expenses");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(e => e.Vendor)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(e => e.Description)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(e => e.ReferenceNumber)
                .HasMaxLength(100);

            builder.Property(e => e.ReceiptUrl)
                .HasMaxLength(500);

            builder.Property(e => e.Tags)
                .HasMaxLength(500);

            builder.Property(e => e.Notes)
                .HasMaxLength(2000);

            builder.Property(e => e.ExpenseDate)
                .IsRequired();

            builder.Property(e => e.Category)
                .IsRequired();

            // Relationships
            builder.HasOne(e => e.Property)
                .WithMany()
                .HasForeignKey(e => e.PropertyId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(e => e.Landlord)
                .WithMany()
                .HasForeignKey(e => e.LandlordId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(e => e.Unit)
                .WithMany()
                .HasForeignKey(e => e.UnitId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            // Indexes for better query performance
            builder.HasIndex(e => e.PropertyId);
            builder.HasIndex(e => e.LandlordId);
            builder.HasIndex(e => e.ExpenseDate);
            builder.HasIndex(e => e.Category);
            builder.HasIndex(e => new { e.PropertyId, e.ExpenseDate });
        }
    }
}
