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
            // ===== SYSTEM ADMIN =====
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
                PropertyId = null, // SystemAdmin sees all properties
                CreatedAt = DateTime.UtcNow
            },

            // ===== LANDLORD 1: John Landlord (Sunset Apartments Westlands) =====
            new User
            {
                FirstName = "John",
                LastName = "Landlord",
                Email = "landlord@example.com",
                PhoneNumber = "+254723870917", // Real number for SMS testing
                // Password: Landlord@123
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Landlord@123"),
                Role = UserRole.Landlord,
                Status = UserStatus.Active,
                PropertyId = null, // Landlords see ALL their owned properties
                CreatedAt = DateTime.UtcNow
            },

            // ===== LANDLORD 2: Mary Wanjiku (Kileleshwa Gardens) =====
            new User
            {
                FirstName = "Mary",
                LastName = "Wanjiku",
                Email = "mary.wanjiku@example.com",
                PhoneNumber = "+254716539952", // Real number for SMS testing
                // Password: Landlord@123
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Landlord@123"),
                Role = UserRole.Landlord,
                Status = UserStatus.Active,
                PropertyId = null, // Landlords see ALL their owned properties
                CreatedAt = DateTime.UtcNow
            },

            // ===== LANDLORD 3: David Kamau (Utawala Maisonettes) =====
            new User
            {
                FirstName = "David",
                LastName = "Kamau",
                Email = "david.kamau@example.com",
                PhoneNumber = "+254734567890",
                // Password: Landlord@123
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Landlord@123"),
                Role = UserRole.Landlord,
                Status = UserStatus.Active,
                PropertyId = null, // Landlords see ALL their owned properties
                CreatedAt = DateTime.UtcNow
            },

            // ===== CARETAKER 1: Jane Mueni (Works for Landlord 1 - Sunset Apartments) =====
            new User
            {
                FirstName = "Jane",
                LastName = "Mueni",
                Email = "caretaker@example.com",
                PhoneNumber = "+254745678901",
                // Password: Caretaker@123
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Caretaker@123"),
                Role = UserRole.Caretaker,
                Status = UserStatus.Active,
                PropertyId = null, // Landlords see ALL their owned properties (same as Landlord 1)
                CreatedAt = DateTime.UtcNow
            },

            // ===== CARETAKER 2: Peter Kamau (Works for Landlord 2 - Kileleshwa) =====
            new User
            {
                FirstName = "Peter",
                LastName = "Kamau",
                Email = "peter.caretaker@example.com",
                PhoneNumber = "+254756789012",
                // Password: Caretaker@123
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Caretaker@123"),
                Role = UserRole.Caretaker,
                Status = UserStatus.Active,
                PropertyId = null, // Landlords see ALL their owned properties (same as Landlord 2)
                CreatedAt = DateTime.UtcNow
            },

            // ===== CARETAKER 3: James Omondi (Works for Landlord 3 - Utawala) =====
            new User
            {
                FirstName = "James",
                LastName = "Omondi",
                Email = "james.caretaker@example.com",
                PhoneNumber = "+254767890123",
                // Password: Caretaker@123
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Caretaker@123"),
                Role = UserRole.Caretaker,
                Status = UserStatus.Active,
                PropertyId = null, // Landlords see ALL their owned properties (same as Landlord 3)
                CreatedAt = DateTime.UtcNow
            },

            // ===== ACCOUNTANT: Grace Wambui (Can view all properties - Read Only) =====
            new User
            {
                FirstName = "Grace",
                LastName = "Wambui",
                Email = "accountant@example.com",
                PhoneNumber = "+254778901234",
                // Password: Accountant@123
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Accountant@123"),
                Role = UserRole.Accountant,
                Status = UserStatus.Active,
                PropertyId = null, // Accountant can view ALL properties (read-only)
                CreatedAt = DateTime.UtcNow
            },

            // ===== TENANTS =====
            // Tenant 1: Peter Mwangi (Unit B1 - Sunset Apartments)
            new User
            {
                FirstName = "Peter",
                LastName = "Mwangi",
                Email = "peter.mwangi@gmail.com",
                PhoneNumber = "+254723870917",
                // Password: Tenant@123
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Tenant@123"),
                Role = UserRole.Tenant,
                Status = UserStatus.Active,
                PropertyId = null,
                CreatedAt = DateTime.UtcNow
            },

            // Tenant 2: Grace Akinyi (Unit 1A - Sunset Apartments)
            new User
            {
                FirstName = "Grace",
                LastName = "Akinyi",
                Email = "grace.akinyi@yahoo.com",
                PhoneNumber = "+254734567890",
                // Password: Tenant@123
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Tenant@123"),
                Role = UserRole.Tenant,
                Status = UserStatus.Active,
                PropertyId = null,
                CreatedAt = DateTime.UtcNow
            },

            // Tenant 3: Alice Wambui (Unit K-2A - Kileleshwa Gardens)
            new User
            {
                FirstName = "Alice",
                LastName = "Wambui",
                Email = "alice.wambui@gmail.com",
                PhoneNumber = "+254745678901",
                // Password: Tenant@123
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Tenant@123"),
                Role = UserRole.Tenant,
                Status = UserStatus.Active,
                PropertyId = null,
                CreatedAt = DateTime.UtcNow
            },

            // Tenant 4: James Kamau (Unit W-3B - Westlands Towers)
            new User
            {
                FirstName = "James",
                LastName = "Kamau",
                Email = "james.kamau@gmail.com",
                PhoneNumber = "+254756789012",
                // Password: Tenant@123
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Tenant@123"),
                Role = UserRole.Tenant,
                Status = UserStatus.Active,
                PropertyId = null,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        logger.LogInformation("Default users seeded successfully");
        logger.LogInformation("=== DEFAULT USER CREDENTIALS ===");
        logger.LogInformation("");
        logger.LogInformation("🔐 SYSTEM ADMIN:");
        logger.LogInformation("   Email: admin@rentcollection.com");
        logger.LogInformation("   Password: Admin@123");
        logger.LogInformation("   Access: ALL properties across all landlords");
        logger.LogInformation("");
        logger.LogInformation("🏢 LANDLORD 1 (John Landlord - Sunset Apartments Westlands):");
        logger.LogInformation("   Email: landlord@example.com");
        logger.LogInformation("   Phone: +254 723 870917 (REAL NUMBER - Can receive SMS)");
        logger.LogInformation("   Password: Landlord@123");
        logger.LogInformation("   Properties: Sunset Apartments Westlands (Bedsitters, One-bedroom)");
        logger.LogInformation("");
        logger.LogInformation("🏢 LANDLORD 2 (Mary Wanjiku - Kileleshwa Gardens):");
        logger.LogInformation("   Email: mary.wanjiku@example.com");
        logger.LogInformation("   Phone: +254 716 539952 (REAL NUMBER - Can receive SMS)");
        logger.LogInformation("   Password: Landlord@123");
        logger.LogInformation("   Properties: Kileleshwa Gardens (Two & Three bedroom)");
        logger.LogInformation("");
        logger.LogInformation("🏢 LANDLORD 3 (David Kamau - Utawala Maisonettes):");
        logger.LogInformation("   Email: david.kamau@example.com");
        logger.LogInformation("   Password: Landlord@123");
        logger.LogInformation("   Properties: Utawala Maisonettes (Own compound)");
        logger.LogInformation("");
        logger.LogInformation("👷 CARETAKER 1 (Jane Mueni - for Landlord 1):");
        logger.LogInformation("   Email: caretaker@example.com");
        logger.LogInformation("   Password: Caretaker@123");
        logger.LogInformation("   Property: Sunset Apartments Westlands");
        logger.LogInformation("");
        logger.LogInformation("👷 CARETAKER 2 (Peter Kamau - for Landlord 2):");
        logger.LogInformation("   Email: peter.caretaker@example.com");
        logger.LogInformation("   Password: Caretaker@123");
        logger.LogInformation("   Property: Kileleshwa Gardens");
        logger.LogInformation("");
        logger.LogInformation("👷 CARETAKER 3 (James Omondi - for Landlord 3):");
        logger.LogInformation("   Email: james.caretaker@example.com");
        logger.LogInformation("   Password: Caretaker@123");
        logger.LogInformation("   Property: Utawala Maisonettes");
        logger.LogInformation("");
        logger.LogInformation("📊 ACCOUNTANT (Grace Wambui):");
        logger.LogInformation("   Email: accountant@example.com");
        logger.LogInformation("   Password: Accountant@123");
        logger.LogInformation("   Access: View ALL properties (read-only financial reports)");
        logger.LogInformation("");
        logger.LogInformation("🏠 TENANTS:");
        logger.LogInformation("   1. Peter Mwangi - peter.mwangi@gmail.com");
        logger.LogInformation("   2. Grace Akinyi - grace.akinyi@yahoo.com");
        logger.LogInformation("   3. Alice Wambui - alice.wambui@gmail.com");
        logger.LogInformation("   4. James Kamau - james.kamau@gmail.com");
        logger.LogInformation("   Password (all): Tenant@123");
        logger.LogInformation("");
        logger.LogInformation("================================");
    }
}
