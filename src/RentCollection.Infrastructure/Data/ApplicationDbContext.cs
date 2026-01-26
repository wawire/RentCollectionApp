using System.Reflection;
using Microsoft.EntityFrameworkCore;
using RentCollection.Domain.Entities;

namespace RentCollection.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Property> Properties { get; set; } = null!;
    public DbSet<Organization> Organizations { get; set; } = null!;
    public DbSet<Unit> Units { get; set; } = null!;
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Invoice> Invoices { get; set; } = null!;
    public DbSet<PaymentAllocation> PaymentAllocations { get; set; } = null!;
    public DbSet<SmsLog> SmsLogs { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Amenity> Amenities { get; set; } = null!;
    public DbSet<PropertyAmenity> PropertyAmenities { get; set; } = null!;
    public DbSet<UnitAmenity> UnitAmenities { get; set; } = null!;
    public DbSet<PropertyImage> PropertyImages { get; set; } = null!;
    public DbSet<UnitImage> UnitImages { get; set; } = null!;
    public DbSet<LandlordPaymentAccount> LandlordPaymentAccounts { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; } = null!;
    public DbSet<Document> Documents { get; set; } = null!;
    public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; } = null!;
    public DbSet<LeaseRenewal> LeaseRenewals { get; set; } = null!;
    public DbSet<SecurityDepositTransaction> SecurityDepositTransactions { get; set; } = null!;
    public DbSet<MPesaTransaction> MPesaTransactions { get; set; } = null!;
    public DbSet<UnmatchedPayment> UnmatchedPayments { get; set; } = null!;
    public DbSet<RentReminder> RentReminders { get; set; } = null!;
    public DbSet<ReminderSettings> ReminderSettings { get; set; } = null!;
    public DbSet<TenantReminderPreference> TenantReminderPreferences { get; set; } = null!;
    public DbSet<Expense> Expenses { get; set; } = null!;
    public DbSet<MoveOutInspection> MoveOutInspections { get; set; } = null!;
    public DbSet<InspectionItem> InspectionItems { get; set; } = null!;
    public DbSet<InspectionPhoto> InspectionPhotos { get; set; } = null!;
    public DbSet<UtilityType> UtilityTypes { get; set; } = null!;
    public DbSet<UtilityConfig> UtilityConfigs { get; set; } = null!;
    public DbSet<MeterReading> MeterReadings { get; set; } = null!;
    public DbSet<InvoiceLineItem> InvoiceLineItems { get; set; } = null!;
    public DbSet<UserPropertyAssignment> UserPropertyAssignments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.Entity<UtilityConfig>(entity =>
        {
            entity.Property(e => e.FixedAmount).HasPrecision(18, 2);
            entity.Property(e => e.Rate).HasPrecision(18, 2);
            entity.Property(e => e.SharedAmount).HasPrecision(18, 2);
            entity.HasOne(e => e.Unit)
                .WithMany()
                .HasForeignKey(e => e.UnitId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<MeterReading>(entity =>
        {
            entity.Property(e => e.ReadingValue).HasPrecision(18, 4);
            entity.HasOne(e => e.UtilityConfig)
                .WithMany(c => c.MeterReadings)
                .HasForeignKey(e => e.UtilityConfigId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Unit)
                .WithMany()
                .HasForeignKey(e => e.UnitId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InvoiceLineItem>(entity =>
        {
            entity.Property(e => e.Quantity).HasPrecision(18, 4);
            entity.Property(e => e.Rate).HasPrecision(18, 2);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                // Only set UpdatedAt if the entity has this property (domain entities, not Identity entities)
                var updatedAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
                if (updatedAtProperty != null)
                {
                    updatedAtProperty.CurrentValue = DateTime.UtcNow;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
