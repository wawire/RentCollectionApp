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
            var roles = new[] { UserRoles.Admin, UserRoles.PropertyManager, UserRoles.Viewer };

            foreach (var roleName in roles)
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

            // Seed Default Admin User
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
                    Role = UserRoles.Admin,
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
                    logger.LogInformation("Default admin user created successfully: {Email}", adminEmail);
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

            // Seed Sample Users
            var sampleUsers = new[]
            {
                new { Email = "manager@rentcollection.com", FirstName = "Property", LastName = "Manager", Role = UserRoles.PropertyManager, Password = "Manager@123" },
                new { Email = "viewer@rentcollection.com", FirstName = "Read", LastName = "Only", Role = UserRoles.Viewer, Password = "Viewer@123" }
            };

            foreach (var userData in sampleUsers)
            {
                var existing = await userManager.FindByEmailAsync(userData.Email);
                if (existing == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = userData.Email,
                        Email = userData.Email,
                        FirstName = userData.FirstName,
                        LastName = userData.LastName,
                        Role = userData.Role,
                        IsActive = true,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(user, userData.Password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, userData.Role);
                        logger.LogInformation("Sample user created: {Email} ({Role})", userData.Email, userData.Role);
                    }
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
