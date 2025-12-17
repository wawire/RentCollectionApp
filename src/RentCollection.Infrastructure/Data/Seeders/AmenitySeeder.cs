using Microsoft.EntityFrameworkCore;
using RentCollection.Domain.Entities;

namespace RentCollection.Infrastructure.Data.Seeders;

public static class AmenitySeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Amenities.AnyAsync())
        {
            return; // Already seeded
        }

        var amenities = new List<Amenity>
        {
            new() { Name = "WiFi", IconName = "FaWifi", IsActive = true },
            new() { Name = "Parking", IconName = "FaParking", IsActive = true },
            new() { Name = "Power Backup", IconName = "FaBolt", IsActive = true },
            new() { Name = "Water Supply", IconName = "FaTint", IsActive = true },
            new() { Name = "Air Conditioning", IconName = "FaSnowflake", IsActive = true },
            new() { Name = "Gas Connection", IconName = "FaFire", IsActive = true },
            new() { Name = "Security", IconName = "FaShieldAlt", IsActive = true },
            new() { Name = "Gym", IconName = "FaDumbbell", IsActive = true },
            new() { Name = "Swimming Pool", IconName = "FaSwimmingPool", IsActive = true },
            new() { Name = "Elevator", IconName = "FaElevator", IsActive = true },
            new() { Name = "Balcony", IconName = "FaBuilding", IsActive = true },
            new() { Name = "Garden", IconName = "FaTree", IsActive = true },
            new() { Name = "Pet Friendly", IconName = "FaDog", IsActive = true },
            new() { Name = "Furnished", IconName = "FaCouch", IsActive = true },
            new() { Name = "Laundry", IconName = "FaTshirt", IsActive = true }
        };

        await context.Amenities.AddRangeAsync(amenities);
        await context.SaveChangesAsync();
    }
}
