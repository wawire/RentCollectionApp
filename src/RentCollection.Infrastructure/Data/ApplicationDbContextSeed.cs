using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;

namespace RentCollection.Infrastructure.Data;

public class ApplicationDbContextSeed
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        try
        {
            // Check if database is already seeded
            if (await context.Properties.AnyAsync())
            {
                logger.LogInformation("Database already contains data. Skipping seed.");
                return;
            }

            logger.LogInformation("Starting database seeding...");

            // Seed Properties
            var properties = new List<Property>
            {
                new Property
                {
                    Name = "Sunset Apartments",
                    Location = "123 Main Street, Nairobi",
                    Description = "Modern apartment complex with amenities",
                    TotalUnits = 10,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Property
                {
                    Name = "Green Valley Residences",
                    Location = "456 Oak Avenue, Westlands",
                    Description = "Family-friendly residential complex",
                    TotalUnits = 8,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Property
                {
                    Name = "City Heights",
                    Location = "789 Urban Road, Kilimani",
                    Description = "Downtown luxury apartments",
                    TotalUnits = 6,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Properties.AddRangeAsync(properties);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} properties", properties.Count);

            // Seed Units for Sunset Apartments
            var sunsetUnits = new List<Unit>
            {
                new Unit
                {
                    UnitNumber = "A101",
                    PropertyId = properties[0].Id,
                    MonthlyRent = 25000,
                    Bedrooms = 2,
                    Bathrooms = 1,
                    SquareFeet = 850,
                    Description = "2-bedroom apartment on ground floor",
                    IsOccupied = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Unit
                {
                    UnitNumber = "A102",
                    PropertyId = properties[0].Id,
                    MonthlyRent = 30000,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    SquareFeet = 1200,
                    Description = "3-bedroom apartment with balcony",
                    IsOccupied = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Unit
                {
                    UnitNumber = "B201",
                    PropertyId = properties[0].Id,
                    MonthlyRent = 28000,
                    Bedrooms = 2,
                    Bathrooms = 2,
                    SquareFeet = 950,
                    Description = "2-bedroom apartment on second floor",
                    IsOccupied = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            // Seed Units for Green Valley
            var greenValleyUnits = new List<Unit>
            {
                new Unit
                {
                    UnitNumber = "101",
                    PropertyId = properties[1].Id,
                    MonthlyRent = 35000,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    SquareFeet = 1400,
                    Description = "Spacious family unit",
                    IsOccupied = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Unit
                {
                    UnitNumber = "102",
                    PropertyId = properties[1].Id,
                    MonthlyRent = 32000,
                    Bedrooms = 2,
                    Bathrooms = 2,
                    SquareFeet = 1100,
                    Description = "Garden view unit",
                    IsOccupied = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            // Seed Units for City Heights
            var cityHeightsUnits = new List<Unit>
            {
                new Unit
                {
                    UnitNumber = "501",
                    PropertyId = properties[2].Id,
                    MonthlyRent = 45000,
                    Bedrooms = 2,
                    Bathrooms = 2,
                    SquareFeet = 1000,
                    Description = "Penthouse unit with city view",
                    IsOccupied = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Unit
                {
                    UnitNumber = "502",
                    PropertyId = properties[2].Id,
                    MonthlyRent = 50000,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    SquareFeet = 1300,
                    Description = "Luxury penthouse",
                    IsOccupied = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            var allUnits = sunsetUnits.Concat(greenValleyUnits).Concat(cityHeightsUnits).ToList();
            await context.Units.AddRangeAsync(allUnits);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} units", allUnits.Count);

            // Seed Sample Tenants
            var tenants = new List<Tenant>
            {
                new Tenant
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com",
                    PhoneNumber = "+254712345678",
                    IdNumber = "12345678",
                    UnitId = sunsetUnits[0].Id,
                    LeaseStartDate = DateTime.UtcNow.AddMonths(-3),
                    LeaseEndDate = DateTime.UtcNow.AddMonths(9),
                    MonthlyRent = 25000,
                    SecurityDeposit = 50000,
                    IsActive = true,
                    Notes = "Excellent tenant, always pays on time",
                    CreatedAt = DateTime.UtcNow.AddMonths(-3)
                },
                new Tenant
                {
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@example.com",
                    PhoneNumber = "+254723456789",
                    IdNumber = "23456789",
                    UnitId = greenValleyUnits[0].Id,
                    LeaseStartDate = DateTime.UtcNow.AddMonths(-6),
                    LeaseEndDate = DateTime.UtcNow.AddMonths(6),
                    MonthlyRent = 35000,
                    SecurityDeposit = 70000,
                    IsActive = true,
                    Notes = "Family with 2 children",
                    CreatedAt = DateTime.UtcNow.AddMonths(-6)
                },
                new Tenant
                {
                    FirstName = "Michael",
                    LastName = "Johnson",
                    Email = "michael.j@example.com",
                    PhoneNumber = "+254734567890",
                    IdNumber = "34567890",
                    UnitId = cityHeightsUnits[0].Id,
                    LeaseStartDate = DateTime.UtcNow.AddMonths(-1),
                    LeaseEndDate = DateTime.UtcNow.AddMonths(11),
                    MonthlyRent = 45000,
                    SecurityDeposit = 90000,
                    IsActive = true,
                    Notes = "Corporate executive",
                    CreatedAt = DateTime.UtcNow.AddMonths(-1)
                }
            };

            await context.Tenants.AddRangeAsync(tenants);

            // Update unit occupancy
            sunsetUnits[0].IsOccupied = true;
            greenValleyUnits[0].IsOccupied = true;
            cityHeightsUnits[0].IsOccupied = true;

            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} tenants", tenants.Count);

            // Seed Sample Payments
            var payments = new List<Payment>
            {
                // John Doe's payments
                new Payment
                {
                    TenantId = tenants[0].Id,
                    Amount = 25000,
                    PaymentDate = DateTime.UtcNow.AddMonths(-3).AddDays(5),
                    PaymentMethod = PaymentMethod.MPesa,
                    Status = PaymentStatus.Completed,
                    TransactionReference = "MPE123456789",
                    PeriodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-3),
                    PeriodEnd = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-2).AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddMonths(-3).AddDays(5)
                },
                new Payment
                {
                    TenantId = tenants[0].Id,
                    Amount = 25000,
                    PaymentDate = DateTime.UtcNow.AddMonths(-2).AddDays(3),
                    PaymentMethod = PaymentMethod.MPesa,
                    Status = PaymentStatus.Completed,
                    TransactionReference = "MPE123456790",
                    PeriodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-2),
                    PeriodEnd = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-1).AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddMonths(-2).AddDays(3)
                },
                new Payment
                {
                    TenantId = tenants[0].Id,
                    Amount = 25000,
                    PaymentDate = DateTime.UtcNow.AddMonths(-1).AddDays(5),
                    PaymentMethod = PaymentMethod.MPesa,
                    Status = PaymentStatus.Completed,
                    TransactionReference = "MPE123456791",
                    PeriodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-1),
                    PeriodEnd = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddMonths(-1).AddDays(5)
                },
                // Jane Smith's payments
                new Payment
                {
                    TenantId = tenants[1].Id,
                    Amount = 35000,
                    PaymentDate = DateTime.UtcNow.AddMonths(-1).AddDays(1),
                    PaymentMethod = PaymentMethod.BankTransfer,
                    Status = PaymentStatus.Completed,
                    TransactionReference = "BANK987654321",
                    PeriodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-1),
                    PeriodEnd = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddMonths(-1).AddDays(1)
                },
                // Michael Johnson's payment
                new Payment
                {
                    TenantId = tenants[2].Id,
                    Amount = 45000,
                    PaymentDate = DateTime.UtcNow.AddDays(-5),
                    PaymentMethod = PaymentMethod.BankTransfer,
                    Status = PaymentStatus.Completed,
                    TransactionReference = "BANK987654322",
                    PeriodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1),
                    PeriodEnd = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1).AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                }
            };

            await context.Payments.AddRangeAsync(payments);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} payments", payments.Count);

            logger.LogInformation("Database seeding completed successfully!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}
