using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RentCollection.Infrastructure.Identity;

namespace RentCollection.Infrastructure.Data;

public class IdentityDataSeeder
{
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger logger)
    {
        try
        {
            logger.LogInformation("Starting Identity data seeding...");

            // Seed Roles
            foreach (var roleName in UserRoles.All)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var role = new IdentityRole(roleName);
                    var result = await roleManager.CreateAsync(role);

                    if (result.Succeeded)
                    {
                        logger.LogInformation("Role '{RoleName}' created successfully", roleName);
                    }
                    else
                    {
                        logger.LogError("Failed to create role '{RoleName}': {Errors}",
                            roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    logger.LogInformation("Role '{RoleName}' already exists", roleName);
                }
            }

            // Seed System Admin User
            var adminEmail = "admin@rentcollection.com";
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    Role = UserRoles.SystemAdmin,
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, UserRoles.SystemAdmin);
                    logger.LogInformation("System admin user created successfully: {Email}", adminEmail);
                    logger.LogWarning("⚠️  DEFAULT ADMIN CREDENTIALS - Email: {Email}, Password: Admin@123", adminEmail);
                    logger.LogWarning("⚠️  PLEASE CHANGE THE DEFAULT PASSWORD IMMEDIATELY!");
                }
                else
                {
                    logger.LogError("Failed to create admin user: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogInformation("Admin user already exists: {Email}", adminEmail);
            }

            // Seed Landlords
            string? landlord1Id = null;
            string? landlord2Id = null;

            var landlord1Email = "landlord1@example.com";
            var landlord1 = await userManager.FindByEmailAsync(landlord1Email);
            if (landlord1 == null)
            {
                landlord1 = new ApplicationUser
                {
                    UserName = landlord1Email,
                    Email = landlord1Email,
                    FirstName = "John",
                    LastName = "Kariuki",
                    Role = UserRoles.Landlord,
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(landlord1, "Landlord@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(landlord1, UserRoles.Landlord);
                    landlord1Id = landlord1.Id;
                    logger.LogInformation("Landlord 1 created: {Email}", landlord1Email);
                }
            }
            else
            {
                landlord1Id = landlord1.Id;
            }

            var landlord2Email = "landlord2@example.com";
            var landlord2 = await userManager.FindByEmailAsync(landlord2Email);
            if (landlord2 == null)
            {
                landlord2 = new ApplicationUser
                {
                    UserName = landlord2Email,
                    Email = landlord2Email,
                    FirstName = "Mary",
                    LastName = "Wanjiku",
                    Role = UserRoles.Landlord,
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(landlord2, "Landlord@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(landlord2, UserRoles.Landlord);
                    landlord2Id = landlord2.Id;
                    logger.LogInformation("Landlord 2 created: {Email}", landlord2Email);
                }
            }
            else
            {
                landlord2Id = landlord2.Id;
            }

            // Seed Caretaker for Landlord 1
            var caretakerEmail = "caretaker@example.com";
            var caretaker = await userManager.FindByEmailAsync(caretakerEmail);
            if (caretaker == null)
            {
                caretaker = new ApplicationUser
                {
                    UserName = caretakerEmail,
                    Email = caretakerEmail,
                    FirstName = "James",
                    LastName = "Omondi",
                    Role = UserRoles.Caretaker,
                    LandlordId = landlord1Id, // Belongs to Landlord 1
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(caretaker, "Caretaker@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(caretaker, UserRoles.Caretaker);
                    logger.LogInformation("Caretaker created: {Email} (Landlord: {LandlordId})", caretakerEmail, landlord1Id);
                }
            }

            // Seed Accountant for Landlord 1
            var accountantEmail = "accountant@example.com";
            var accountant = await userManager.FindByEmailAsync(accountantEmail);
            if (accountant == null)
            {
                accountant = new ApplicationUser
                {
                    UserName = accountantEmail,
                    Email = accountantEmail,
                    FirstName = "Grace",
                    LastName = "Mutua",
                    Role = UserRoles.Accountant,
                    LandlordId = landlord1Id, // Belongs to Landlord 1
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(accountant, "Accountant@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(accountant, UserRoles.Accountant);
                    logger.LogInformation("Accountant created: {Email} (Landlord: {LandlordId})", accountantEmail, landlord1Id);
                }
            }

            logger.LogInformation("Identity data seeding completed successfully!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding Identity data");
            throw;
        }
    }
}
