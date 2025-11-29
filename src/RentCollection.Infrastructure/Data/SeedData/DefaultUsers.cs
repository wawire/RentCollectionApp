using Microsoft.Extensions.Logging;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;

namespace RentCollection.Infrastructure.Data.SeedData;

/// <summary>
/// Seeds default users for development and testing
/// </summary>
public static class DefaultUsers
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        // Check if any users exist
        if (context.Users.Any())
        {
            logger.LogInformation("Users already seeded");
            return;
        }

        logger.LogInformation("Seeding default users...");

        var users = new List<User>
        {
            // System Admin
            new User
            {
                FirstName = "System",
                LastName = "Administrator",
                Email = "admin@rentcollection.com",
                PhoneNumber = "+254700000000",
                // Password: Admin@123
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.SystemAdmin,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            },
            // Demo Landlord
            new User
            {
                FirstName = "John",
                LastName = "Landlord",
                Email = "landlord@example.com",
                PhoneNumber = "+254712345678",
                // Password: Landlord@123
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Landlord@123"),
                Role = UserRole.Landlord,
                Status = UserStatus.Active,
                PropertyId = 1, // Will be linked to first property
                CreatedAt = DateTime.UtcNow
            },
            // Demo Caretaker
            new User
            {
                FirstName = "Mary",
                LastName = "Caretaker",
                Email = "caretaker@example.com",
                PhoneNumber = "+254723456789",
                // Password: Caretaker@123
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Caretaker@123"),
                Role = UserRole.Caretaker,
                Status = UserStatus.Active,
                PropertyId = 1, // Will be linked to first property
                CreatedAt = DateTime.UtcNow
            },
            // Demo Accountant
            new User
            {
                FirstName = "James",
                LastName = "Accountant",
                Email = "accountant@example.com",
                PhoneNumber = "+254734567890",
                // Password: Accountant@123
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Accountant@123"),
                Role = UserRole.Accountant,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        logger.LogInformation("Default users seeded successfully");
        logger.LogInformation("=== Default User Credentials ===");
        logger.LogInformation("System Admin: admin@rentcollection.com / Admin@123");
        logger.LogInformation("Landlord: landlord@example.com / Landlord@123");
        logger.LogInformation("Caretaker: caretaker@example.com / Caretaker@123");
        logger.LogInformation("Accountant: accountant@example.com / Accountant@123");
        logger.LogInformation("================================");
    }
}
