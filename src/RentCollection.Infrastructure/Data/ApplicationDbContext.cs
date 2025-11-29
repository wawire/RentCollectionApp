using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RentCollection.Domain.Entities;
using RentCollection.Infrastructure.Identity;

namespace RentCollection.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Property> Properties { get; set; } = null!;
    public DbSet<Unit> Units { get; set; } = null!;
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<SmsLog> SmsLogs { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
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
