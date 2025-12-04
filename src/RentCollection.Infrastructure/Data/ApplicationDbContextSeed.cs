using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data.SeedData;

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

            // Get landlords (users should already be seeded)
            var johnLandlord = await context.Users.FirstOrDefaultAsync(u => u.Email == "landlord@example.com");
            var maryLandlord = await context.Users.FirstOrDefaultAsync(u => u.Email == "mary.wanjiku@example.com");
            var davidLandlord = await context.Users.FirstOrDefaultAsync(u => u.Email == "david.kamau@example.com");

            if (johnLandlord == null || maryLandlord == null || davidLandlord == null)
            {
                logger.LogError("Landlord users not found! Please ensure users are seeded first.");
                throw new InvalidOperationException("Landlord users must be seeded before properties.");
            }

            // ===== SEED PROPERTIES (3 Landlords, 6 Properties) =====
            var properties = new List<Property>
            {
                // LANDLORD 1 (John Landlord) - Properties in Westlands & Parklands
                new Property
                {
                    Name = "Sunset Apartments Westlands",
                    Location = "Muthangari Road, Westlands, Nairobi",
                    Description = "Modern bedsitters and one-bedroom apartments in the heart of Westlands. Walking distance to Sarit Centre and Westgate Mall. Secure compound with ample parking.",
                    TotalUnits = 12,
                    LandlordId = johnLandlord.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Property
                {
                    Name = "Parklands Heights",
                    Location = "5th Avenue, Parklands, Nairobi",
                    Description = "Affordable bedsitters and studios for young professionals. Near shops, hospitals, and public transport. Water available 24/7.",
                    TotalUnits = 8,
                    LandlordId = johnLandlord.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                // LANDLORD 2 (Mary Wanjiku) - Properties in Kileleshwa & Lavington
                new Property
                {
                    Name = "Kileleshwa Gardens",
                    Location = "Mandera Road, Kileleshwa, Nairobi",
                    Description = "Family-friendly two and three bedroom apartments with spacious balconies. Quiet neighborhood with schools nearby. Borehole water and backup generator.",
                    TotalUnits = 15,
                    LandlordId = maryLandlord.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Property
                {
                    Name = "Lavington Court",
                    Location = "James Gichuru Road, Lavington, Nairobi",
                    Description = "Premium two and three bedroom apartments in serene Lavington. Swimming pool, gym, children's play area. DSTV ready, fiber internet.",
                    TotalUnits = 10,
                    LandlordId = maryLandlord.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                // LANDLORD 3 (David Kamau) - Own Compound Houses in Utawala & Ruiru
                new Property
                {
                    Name = "Utawala Maisonettes",
                    Location = "Eastern Bypass, Utawala, Nairobi",
                    Description = "Own compound maisonettes with 3 bedrooms, DSQ (domestic servant quarters), parking for 2 cars. Gated community with 24hr security.",
                    TotalUnits = 6,
                    LandlordId = davidLandlord.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Property
                {
                    Name = "Ruiru Bungalows Estate",
                    Location = "Ruiru-Kiambu Road, Ruiru",
                    Description = "Standalone bungalows (own compound) with 2-3 bedrooms. Each house has a garden, parking, and perimeter wall. Perfect for families seeking peace and privacy.",
                    TotalUnits = 8,
                    LandlordId = davidLandlord.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Properties.AddRangeAsync(properties);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} properties", properties.Count);

            // ===== SEED AMENITIES =====
            var amenities = new List<Amenity>
            {
                new Amenity
                {
                    Name = "WiFi",
                    IconName = "Wifi",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Amenity
                {
                    Name = "Parking",
                    IconName = "Car",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Amenity
                {
                    Name = "Power Backup",
                    IconName = "Zap",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Amenity
                {
                    Name = "Water Supply",
                    IconName = "Droplets",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Amenity
                {
                    Name = "Air Conditioning",
                    IconName = "Wind",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Amenity
                {
                    Name = "Gas Connection",
                    IconName = "Flame",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Amenity
                {
                    Name = "Security",
                    IconName = "Shield",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Amenities.AddRangeAsync(amenities);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} amenities", amenities.Count);

            // ===== SEED PROPERTY AMENITIES (Assign amenities to properties) =====
            var propertyAmenities = new List<PropertyAmenity>
            {
                // Sunset Apartments Westlands - WiFi, Parking, Security
                new PropertyAmenity { PropertyId = properties[0].Id, AmenityId = amenities[0].Id, CreatedAt = DateTime.UtcNow },
                new PropertyAmenity { PropertyId = properties[0].Id, AmenityId = amenities[1].Id, CreatedAt = DateTime.UtcNow },
                new PropertyAmenity { PropertyId = properties[0].Id, AmenityId = amenities[6].Id, CreatedAt = DateTime.UtcNow },

                // Parklands Heights - Parking, Water Supply, Security
                new PropertyAmenity { PropertyId = properties[1].Id, AmenityId = amenities[1].Id, CreatedAt = DateTime.UtcNow },
                new PropertyAmenity { PropertyId = properties[1].Id, AmenityId = amenities[3].Id, CreatedAt = DateTime.UtcNow },
                new PropertyAmenity { PropertyId = properties[1].Id, AmenityId = amenities[6].Id, CreatedAt = DateTime.UtcNow },

                // Kileleshwa Gardens - WiFi, Parking, Power Backup, Water Supply, Security
                new PropertyAmenity { PropertyId = properties[2].Id, AmenityId = amenities[0].Id, CreatedAt = DateTime.UtcNow },
                new PropertyAmenity { PropertyId = properties[2].Id, AmenityId = amenities[1].Id, CreatedAt = DateTime.UtcNow },
                new PropertyAmenity { PropertyId = properties[2].Id, AmenityId = amenities[2].Id, CreatedAt = DateTime.UtcNow },
                new PropertyAmenity { PropertyId = properties[2].Id, AmenityId = amenities[3].Id, CreatedAt = DateTime.UtcNow },
                new PropertyAmenity { PropertyId = properties[2].Id, AmenityId = amenities[6].Id, CreatedAt = DateTime.UtcNow },

                // Lavington Court - All amenities (Premium property)
                new PropertyAmenity { PropertyId = properties[3].Id, AmenityId = amenities[0].Id, CreatedAt = DateTime.UtcNow },
                new PropertyAmenity { PropertyId = properties[3].Id, AmenityId = amenities[1].Id, CreatedAt = DateTime.UtcNow },
                new PropertyAmenity { PropertyId = properties[3].Id, AmenityId = amenities[2].Id, CreatedAt = DateTime.UtcNow },
                new PropertyAmenity { PropertyId = properties[3].Id, AmenityId = amenities[3].Id, CreatedAt = DateTime.UtcNow },
                new PropertyAmenity { PropertyId = properties[3].Id, AmenityId = amenities[4].Id, CreatedAt = DateTime.UtcNow },
                new PropertyAmenity { PropertyId = properties[3].Id, AmenityId = amenities[5].Id, CreatedAt = DateTime.UtcNow },
                new PropertyAmenity { PropertyId = properties[3].Id, AmenityId = amenities[6].Id, CreatedAt = DateTime.UtcNow },

                // Utawala Maisonettes - Parking, Water Supply, Security
                new PropertyAmenity { PropertyId = properties[4].Id, AmenityId = amenities[1].Id, CreatedAt = DateTime.UtcNow },
                new PropertyAmenity { PropertyId = properties[4].Id, AmenityId = amenities[3].Id, CreatedAt = DateTime.UtcNow },
                new PropertyAmenity { PropertyId = properties[4].Id, AmenityId = amenities[6].Id, CreatedAt = DateTime.UtcNow },

                // Ruiru Bungalows - Parking, Water Supply, Security
                new PropertyAmenity { PropertyId = properties[5].Id, AmenityId = amenities[1].Id, CreatedAt = DateTime.UtcNow },
                new PropertyAmenity { PropertyId = properties[5].Id, AmenityId = amenities[3].Id, CreatedAt = DateTime.UtcNow },
                new PropertyAmenity { PropertyId = properties[5].Id, AmenityId = amenities[6].Id, CreatedAt = DateTime.UtcNow },
            };

            await context.PropertyAmenities.AddRangeAsync(propertyAmenities);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} property amenities relationships", propertyAmenities.Count);

            // ===== SEED UNITS WITH KENYAN PROPERTY TYPES =====

            // PROPERTY 1: Sunset Apartments Westlands (Bedsitters & One-Bedroom)
            var sunsetUnits = new List<Unit>
            {
                // Bedsitters (Studio apartments)
                new Unit
                {
                    UnitNumber = "B1",
                    PropertyId = properties[0].Id,
                    MonthlyRent = 12000,
                    Bedrooms = 0, // Bedsitter (studio)
                    Bathrooms = 1,
                    SquareFeet = 300,
                    Description = "Bedsitter with own bathroom and kitchenette. Ground floor.",
                    IsOccupied = true,
                    IsActive = true,
                    RentalType = RentalType.Rent,
                    CreatedAt = DateTime.UtcNow
                },
                new Unit
                {
                    UnitNumber = "B2",
                    PropertyId = properties[0].Id,
                    MonthlyRent = 12000,
                    Bedrooms = 0,
                    Bathrooms = 1,
                    SquareFeet = 300,
                    Description = "Bedsitter with balcony. First floor.",
                    IsOccupied = false,
                    IsActive = true,
                    RentalType = RentalType.Rent,
                    CreatedAt = DateTime.UtcNow
                },
                new Unit
                {
                    UnitNumber = "B3",
                    PropertyId = properties[0].Id,
                    MonthlyRent = 13000,
                    Bedrooms = 0,
                    Bathrooms = 1,
                    SquareFeet = 320,
                    Description = "Spacious bedsitter. Second floor with city view.",
                    IsOccupied = false,
                    IsActive = true,
                    RentalType = RentalType.Rent,
                    CreatedAt = DateTime.UtcNow
                },
                // One-Bedroom
                new Unit
                {
                    UnitNumber = "1A",
                    PropertyId = properties[0].Id,
                    MonthlyRent = 18000,
                    Bedrooms = 1,
                    Bathrooms = 1,
                    SquareFeet = 450,
                    Description = "One bedroom with separate kitchen and sitting room.",
                    IsOccupied = true,
                    IsActive = true,
                    RentalType = RentalType.Both,
                    CreatedAt = DateTime.UtcNow
                },
                new Unit
                {
                    UnitNumber = "1B",
                    PropertyId = properties[0].Id,
                    MonthlyRent = 18000,
                    Bedrooms = 1,
                    Bathrooms = 1,
                    SquareFeet = 450,
                    Description = "One bedroom apartment with balcony.",
                    IsOccupied = false,
                    IsActive = true,
                    RentalType = RentalType.Both,
                    CreatedAt = DateTime.UtcNow
                }
            };

            // PROPERTY 2: Parklands Heights (Bedsitters)
            var parklandsUnits = new List<Unit>
            {
                new Unit
                {
                    UnitNumber = "P1",
                    PropertyId = properties[1].Id,
                    MonthlyRent = 10000,
                    Bedrooms = 0,
                    Bathrooms = 1,
                    SquareFeet = 280,
                    Description = "Affordable bedsitter for students/young professionals.",
                    IsOccupied = true,
                    IsActive = true,
                    RentalType = RentalType.Rent,
                    CreatedAt = DateTime.UtcNow
                },
                new Unit
                {
                    UnitNumber = "P2",
                    PropertyId = properties[1].Id,
                    MonthlyRent = 10000,
                    Bedrooms = 0,
                    Bathrooms = 1,
                    SquareFeet = 280,
                    Description = "Bedsitter with shared laundry area.",
                    IsOccupied = false,
                    IsActive = true,
                    RentalType = RentalType.Rent,
                    CreatedAt = DateTime.UtcNow
                },
                new Unit
                {
                    UnitNumber = "P3",
                    PropertyId = properties[1].Id,
                    MonthlyRent = 11000,
                    Bedrooms = 1,
                    Bathrooms = 1,
                    SquareFeet = 400,
                    Description = "One bedroom apartment.",
                    IsOccupied = false,
                    IsActive = true,
                    RentalType = RentalType.Both,
                    CreatedAt = DateTime.UtcNow
                }
            };

            // PROPERTY 3: Kileleshwa Gardens (Two & Three Bedroom)
            var kileleshwaUnits = new List<Unit>
            {
                new Unit
                {
                    UnitNumber = "2A",
                    PropertyId = properties[2].Id,
                    MonthlyRent = 35000,
                    Bedrooms = 2,
                    Bathrooms = 2,
                    SquareFeet = 900,
                    Description = "Two bedroom apartment with master ensuite. DSQ included.",
                    IsOccupied = true,
                    IsActive = true,
                    RentalType = RentalType.Both,
                    CreatedAt = DateTime.UtcNow
                },
                new Unit
                {
                    UnitNumber = "2B",
                    PropertyId = properties[2].Id,
                    MonthlyRent = 35000,
                    Bedrooms = 2,
                    Bathrooms = 2,
                    SquareFeet = 900,
                    Description = "Two bedroom with balcony facing garden.",
                    IsOccupied = false,
                    IsActive = true,
                    RentalType = RentalType.Both,
                    CreatedAt = DateTime.UtcNow
                },
                new Unit
                {
                    UnitNumber = "3A",
                    PropertyId = properties[2].Id,
                    MonthlyRent = 45000,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    SquareFeet = 1200,
                    Description = "Three bedroom apartment with master ensuite and DSQ.",
                    IsOccupied = true,
                    IsActive = true,
                    RentalType = RentalType.Lease,
                    CreatedAt = DateTime.UtcNow
                },
                new Unit
                {
                    UnitNumber = "3B",
                    PropertyId = properties[2].Id,
                    MonthlyRent = 45000,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    SquareFeet = 1200,
                    Description = "Spacious three bedroom with dining area.",
                    IsOccupied = false,
                    IsActive = true,
                    RentalType = RentalType.Lease,
                    CreatedAt = DateTime.UtcNow
                }
            };

            // PROPERTY 4: Lavington Court (Premium Two & Three Bedroom)
            var lavingtonUnits = new List<Unit>
            {
                new Unit
                {
                    UnitNumber = "L201",
                    PropertyId = properties[3].Id,
                    MonthlyRent = 55000,
                    Bedrooms = 2,
                    Bathrooms = 2,
                    SquareFeet = 1000,
                    Description = "Premium two bedroom with gym and pool access. Fiber internet.",
                    IsOccupied = false,
                    IsActive = true,
                    RentalType = RentalType.Both,
                    CreatedAt = DateTime.UtcNow
                },
                new Unit
                {
                    UnitNumber = "L301",
                    PropertyId = properties[3].Id,
                    MonthlyRent = 70000,
                    Bedrooms = 3,
                    Bathrooms = 3,
                    SquareFeet = 1400,
                    Description = "Luxury three bedroom with all bedrooms ensuite. DSQ, balcony.",
                    IsOccupied = true,
                    IsActive = true,
                    RentalType = RentalType.Lease,
                    CreatedAt = DateTime.UtcNow
                }
            };

            // PROPERTY 5: Utawala Maisonettes (Own Compound - Maisonettes)
            var utawalaUnits = new List<Unit>
            {
                new Unit
                {
                    UnitNumber = "M1",
                    PropertyId = properties[4].Id,
                    MonthlyRent = 40000,
                    Bedrooms = 3,
                    Bathrooms = 3,
                    SquareFeet = 1500,
                    Description = "Three bedroom maisonette (own compound) with DSQ, parking for 2 cars, garden.",
                    IsOccupied = true,
                    IsActive = true,
                    RentalType = RentalType.Lease,
                    CreatedAt = DateTime.UtcNow
                },
                new Unit
                {
                    UnitNumber = "M2",
                    PropertyId = properties[4].Id,
                    MonthlyRent = 40000,
                    Bedrooms = 3,
                    Bathrooms = 3,
                    SquareFeet = 1500,
                    Description = "Maisonette with own gate, perimeter wall, and compound. DSQ included.",
                    IsOccupied = false,
                    IsActive = true,
                    RentalType = RentalType.Lease,
                    CreatedAt = DateTime.UtcNow
                },
                new Unit
                {
                    UnitNumber = "M3",
                    PropertyId = properties[4].Id,
                    MonthlyRent = 42000,
                    Bedrooms = 3,
                    Bathrooms = 3,
                    SquareFeet = 1600,
                    Description = "Corner maisonette with larger compound and garden space.",
                    IsOccupied = false,
                    IsActive = true,
                    RentalType = RentalType.Lease,
                    CreatedAt = DateTime.UtcNow
                }
            };

            // PROPERTY 6: Ruiru Bungalows (Own Compound - Bungalows)
            var ruiruUnits = new List<Unit>
            {
                new Unit
                {
                    UnitNumber = "BG1",
                    PropertyId = properties[5].Id,
                    MonthlyRent = 30000,
                    Bedrooms = 2,
                    Bathrooms = 2,
                    SquareFeet = 1000,
                    Description = "Two bedroom bungalow (own compound) with garden, parking, perimeter wall.",
                    IsOccupied = true,
                    IsActive = true,
                    RentalType = RentalType.Both,
                    CreatedAt = DateTime.UtcNow
                },
                new Unit
                {
                    UnitNumber = "BG2",
                    PropertyId = properties[5].Id,
                    MonthlyRent = 30000,
                    Bedrooms = 2,
                    Bathrooms = 2,
                    SquareFeet = 1000,
                    Description = "Standalone bungalow with own compound. Family-friendly.",
                    IsOccupied = false,
                    IsActive = true,
                    RentalType = RentalType.Both,
                    CreatedAt = DateTime.UtcNow
                },
                new Unit
                {
                    UnitNumber = "BG3",
                    PropertyId = properties[5].Id,
                    MonthlyRent = 35000,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    SquareFeet = 1200,
                    Description = "Three bedroom bungalow with spacious compound and kitchen garden.",
                    IsOccupied = true,
                    IsActive = true,
                    RentalType = RentalType.Lease,
                    CreatedAt = DateTime.UtcNow
                }
            };

            var allUnits = sunsetUnits
                .Concat(parklandsUnits)
                .Concat(kileleshwaUnits)
                .Concat(lavingtonUnits)
                .Concat(utawalaUnits)
                .Concat(ruiruUnits)
                .ToList();

            await context.Units.AddRangeAsync(allUnits);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} units", allUnits.Count);

            // ===== SEED SAMPLE TENANTS =====
            var tenants = new List<Tenant>
            {
                // Tenants for Property 1 (Sunset Apartments Westlands)
                new Tenant
                {
                    FirstName = "Peter",
                    LastName = "Mwangi",
                    Email = "peter.mwangi@gmail.com",
                    PhoneNumber = "+254723870917", // Real number for SMS testing
                    IdNumber = "28934567",
                    UnitId = sunsetUnits[0].Id, // B1 Bedsitter
                    LeaseStartDate = DateTime.UtcNow.AddMonths(-6),
                    LeaseEndDate = DateTime.UtcNow.AddMonths(6),
                    MonthlyRent = 12000,
                    SecurityDeposit = 24000,
                    IsActive = true,
                    Notes = "University graduate, IT professional",
                    CreatedAt = DateTime.UtcNow.AddMonths(-6)
                },
                new Tenant
                {
                    FirstName = "Grace",
                    LastName = "Akinyi",
                    Email = "grace.akinyi@yahoo.com",
                    PhoneNumber = "+254716539952", // Real number for SMS testing
                    IdNumber = "31245678",
                    UnitId = sunsetUnits[3].Id, // 1A One-bedroom
                    LeaseStartDate = DateTime.UtcNow.AddMonths(-3),
                    LeaseEndDate = DateTime.UtcNow.AddMonths(9),
                    MonthlyRent = 18000,
                    SecurityDeposit = 36000,
                    IsActive = true,
                    Notes = "Marketing executive, always pays early",
                    CreatedAt = DateTime.UtcNow.AddMonths(-3)
                },

                // Tenants for Property 2 (Parklands Heights)
                new Tenant
                {
                    FirstName = "James",
                    LastName = "Ochieng",
                    Email = "james.o@gmail.com",
                    PhoneNumber = "+254734567890",
                    IdNumber = "25678901",
                    UnitId = parklandsUnits[0].Id, // P1 Bedsitter
                    LeaseStartDate = DateTime.UtcNow.AddMonths(-4),
                    LeaseEndDate = DateTime.UtcNow.AddMonths(8),
                    MonthlyRent = 10000,
                    SecurityDeposit = 20000,
                    IsActive = true,
                    Notes = "Student at UoN, reliable tenant",
                    CreatedAt = DateTime.UtcNow.AddMonths(-4)
                },

                // Tenants for Property 3 (Kileleshwa Gardens)
                new Tenant
                {
                    FirstName = "Alice",
                    LastName = "Wambui",
                    Email = "alice.wambui@gmail.com",
                    PhoneNumber = "+254745678901",
                    IdNumber = "27890123",
                    UnitId = kileleshwaUnits[0].Id, // 2A Two-bedroom
                    LeaseStartDate = DateTime.UtcNow.AddMonths(-8),
                    LeaseEndDate = DateTime.UtcNow.AddMonths(4),
                    MonthlyRent = 35000,
                    SecurityDeposit = 70000,
                    IsActive = true,
                    Notes = "Family with 2 kids, very clean",
                    CreatedAt = DateTime.UtcNow.AddMonths(-8)
                },
                new Tenant
                {
                    FirstName = "Michael",
                    LastName = "Kimani",
                    Email = "mkimani@outlook.com",
                    PhoneNumber = "+254756789012",
                    IdNumber = "29012345",
                    UnitId = kileleshwaUnits[2].Id, // 3A Three-bedroom
                    LeaseStartDate = DateTime.UtcNow.AddMonths(-5),
                    LeaseEndDate = DateTime.UtcNow.AddMonths(7),
                    MonthlyRent = 45000,
                    SecurityDeposit = 90000,
                    IsActive = true,
                    Notes = "Bank manager, excellent payment record",
                    CreatedAt = DateTime.UtcNow.AddMonths(-5)
                },

                // Tenants for Property 4 (Lavington Court)
                new Tenant
                {
                    FirstName = "Sarah",
                    LastName = "Njeri",
                    Email = "sarah.njeri@gmail.com",
                    PhoneNumber = "+254767890123",
                    IdNumber = "30123456",
                    UnitId = lavingtonUnits[1].Id, // L301 Luxury three-bedroom
                    LeaseStartDate = DateTime.UtcNow.AddMonths(-2),
                    LeaseEndDate = DateTime.UtcNow.AddMonths(10),
                    MonthlyRent = 70000,
                    SecurityDeposit = 140000,
                    IsActive = true,
                    Notes = "Corporate lawyer, pays via bank transfer",
                    CreatedAt = DateTime.UtcNow.AddMonths(-2)
                },

                // Tenants for Property 5 (Utawala Maisonettes)
                new Tenant
                {
                    FirstName = "Daniel",
                    LastName = "Otieno",
                    Email = "daniel.otieno@gmail.com",
                    PhoneNumber = "+254778901234",
                    IdNumber = "32234567",
                    UnitId = utawalaUnits[0].Id, // M1 Maisonette
                    LeaseStartDate = DateTime.UtcNow.AddMonths(-7),
                    LeaseEndDate = DateTime.UtcNow.AddMonths(5),
                    MonthlyRent = 40000,
                    SecurityDeposit = 80000,
                    IsActive = true,
                    Notes = "Family with 3 children, needs house help",
                    CreatedAt = DateTime.UtcNow.AddMonths(-7)
                },

                // Tenants for Property 6 (Ruiru Bungalows)
                new Tenant
                {
                    FirstName = "Lucy",
                    LastName = "Wanjiru",
                    Email = "lucy.w@gmail.com",
                    PhoneNumber = "+254789012345",
                    IdNumber = "26345678",
                    UnitId = ruiruUnits[0].Id, // BG1 Two-bedroom bungalow
                    LeaseStartDate = DateTime.UtcNow.AddMonths(-10),
                    LeaseEndDate = DateTime.UtcNow.AddMonths(2),
                    MonthlyRent = 30000,
                    SecurityDeposit = 60000,
                    IsActive = true,
                    Notes = "Teacher, quiet family",
                    CreatedAt = DateTime.UtcNow.AddMonths(-10)
                },
                new Tenant
                {
                    FirstName = "Joseph",
                    LastName = "Mutua",
                    Email = "joseph.mutua@gmail.com",
                    PhoneNumber = "+254790123456",
                    IdNumber = "33456789",
                    UnitId = ruiruUnits[2].Id, // BG3 Three-bedroom bungalow
                    LeaseStartDate = DateTime.UtcNow.AddMonths(-4),
                    LeaseEndDate = DateTime.UtcNow.AddMonths(8),
                    MonthlyRent = 35000,
                    SecurityDeposit = 70000,
                    IsActive = true,
                    Notes = "Civil servant, has vegetable garden",
                    CreatedAt = DateTime.UtcNow.AddMonths(-4)
                }
            };

            await context.Tenants.AddRangeAsync(tenants);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} tenants", tenants.Count);

            // ===== UPDATE UNITS WITH PAYMENT ACCOUNT NUMBERS =====
            // Assign payment account numbers to units for M-Pesa Paybill identification
            sunsetUnits[0].PaymentAccountNumber = "B1";
            sunsetUnits[1].PaymentAccountNumber = "B2";
            sunsetUnits[2].PaymentAccountNumber = "B3";
            sunsetUnits[3].PaymentAccountNumber = "1A";
            sunsetUnits[4].PaymentAccountNumber = "1B";

            parklandsUnits[0].PaymentAccountNumber = "P1";
            parklandsUnits[1].PaymentAccountNumber = "P2";
            parklandsUnits[2].PaymentAccountNumber = "P3";

            kileleshwaUnits[0].PaymentAccountNumber = "K-2A";
            kileleshwaUnits[1].PaymentAccountNumber = "K-2B";
            kileleshwaUnits[2].PaymentAccountNumber = "K-3A";
            kileleshwaUnits[3].PaymentAccountNumber = "K-3B";

            lavingtonUnits[0].PaymentAccountNumber = "L201";
            lavingtonUnits[1].PaymentAccountNumber = "L301";

            utawalaUnits[0].PaymentAccountNumber = "M1";
            utawalaUnits[1].PaymentAccountNumber = "M2";
            utawalaUnits[2].PaymentAccountNumber = "M3";

            ruiruUnits[0].PaymentAccountNumber = "BG1";
            ruiruUnits[1].PaymentAccountNumber = "BG2";
            ruiruUnits[2].PaymentAccountNumber = "BG3";

            context.Units.UpdateRange(allUnits);
            await context.SaveChangesAsync();
            logger.LogInformation("Updated units with payment account numbers");

            // ===== SEED LANDLORD PAYMENT ACCOUNTS =====
            var paymentAccounts = new List<LandlordPaymentAccount>
            {
                // John Landlord - M-Pesa Paybill for Sunset Apartments
                new LandlordPaymentAccount
                {
                    LandlordId = johnLandlord.Id,
                    PropertyId = properties[0].Id, // Sunset Apartments
                    AccountName = "Sunset Apartments M-Pesa",
                    AccountType = PaymentAccountType.MPesaPaybill,
                    PaybillNumber = "123456",
                    PaybillName = "John Landlord Properties",
                    IsDefault = true,
                    IsActive = true,
                    AutoReconciliation = false,
                    PaymentInstructions = "Pay to M-Pesa Paybill 123456. Use your unit number (e.g., B1, 1A) as the Account Number.",
                    CreatedAt = DateTime.UtcNow
                },

                // John Landlord - Equity Bank for Parklands Heights
                new LandlordPaymentAccount
                {
                    LandlordId = johnLandlord.Id,
                    PropertyId = properties[1].Id, // Parklands Heights
                    AccountName = "Parklands Heights Bank Account",
                    AccountType = PaymentAccountType.BankAccount,
                    BankName = "Equity Bank",
                    BankAccountNumber = "0150123456789",
                    BankAccountName = "John Landlord",
                    BankBranch = "Westlands Branch",
                    SwiftCode = "EQBLKENA",
                    IsDefault = false,
                    IsActive = true,
                    PaymentInstructions = "Transfer to Equity Bank Account 0150123456789. Use 'RENT-[YOUR UNIT NUMBER]' as reference (e.g., RENT-P1).",
                    CreatedAt = DateTime.UtcNow
                },

                // Mary Wanjiku - M-Pesa Paybill for Kileleshwa Gardens
                new LandlordPaymentAccount
                {
                    LandlordId = maryLandlord.Id,
                    PropertyId = properties[2].Id, // Kileleshwa Gardens
                    AccountName = "Kileleshwa Gardens Paybill",
                    AccountType = PaymentAccountType.MPesaPaybill,
                    PaybillNumber = "789012",
                    PaybillName = "Mary Wanjiku Rentals",
                    IsDefault = true,
                    IsActive = true,
                    AutoReconciliation = false,
                    PaymentInstructions = "M-Pesa Paybill 789012. Account Number: Your unit (K-2A, K-3A, etc.).",
                    CreatedAt = DateTime.UtcNow
                },

                // Mary Wanjiku - KCB Bank for Lavington Court
                new LandlordPaymentAccount
                {
                    LandlordId = maryLandlord.Id,
                    PropertyId = properties[3].Id, // Lavington Court
                    AccountName = "Lavington Court KCB",
                    AccountType = PaymentAccountType.BankAccount,
                    BankName = "KCB Bank",
                    BankAccountNumber = "1234567890",
                    BankAccountName = "Mary Wanjiku",
                    BankBranch = "Lavington Branch",
                    SwiftCode = "KCBLKENX",
                    IsDefault = true,
                    IsActive = true,
                    PaymentInstructions = "KCB Bank 1234567890. Reference: RENT-[UNIT] (e.g., RENT-L201).",
                    CreatedAt = DateTime.UtcNow
                },

                // David Kamau - M-Pesa Paybill for all properties
                new LandlordPaymentAccount
                {
                    LandlordId = davidLandlord.Id,
                    PropertyId = null, // General account for all properties
                    AccountName = "David Kamau Properties M-Pesa",
                    AccountType = PaymentAccountType.MPesaPaybill,
                    PaybillNumber = "345678",
                    PaybillName = "David Kamau Rentals",
                    IsDefault = true,
                    IsActive = true,
                    AutoReconciliation = false,
                    PaymentInstructions = "M-Pesa Paybill 345678. Use your unit number as Account (M1, BG1, etc.).",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.LandlordPaymentAccounts.AddRangeAsync(paymentAccounts);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} landlord payment accounts", paymentAccounts.Count);

            // ===== SEED SAMPLE PAYMENTS =====
            var payments = new List<Payment>
            {
                // Peter Mwangi (Bedsitter KSh 12,000) - M-Pesa Paybill
                new Payment
                {
                    TenantId = tenants[0].Id,
                    UnitId = sunsetUnits[0].Id,
                    LandlordAccountId = paymentAccounts[0].Id, // Sunset Apartments M-Pesa
                    Amount = 12000,
                    PaymentDate = DateTime.UtcNow.AddMonths(-1).AddDays(3),
                    PaymentMethod = PaymentMethod.MPesa,
                    Status = PaymentStatus.Completed,
                    TransactionReference = "QHK2NP9X7M",
                    PaybillAccountNumber = "B1",
                    MPesaPhoneNumber = "+254723870917",
                    PeriodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-1),
                    PeriodEnd = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddMonths(-1).AddDays(3)
                },

                // Grace Akinyi (One-bedroom KSh 18,000) - M-Pesa Paybill
                new Payment
                {
                    TenantId = tenants[1].Id,
                    UnitId = sunsetUnits[3].Id,
                    LandlordAccountId = paymentAccounts[0].Id,
                    Amount = 18000,
                    PaymentDate = DateTime.UtcNow.AddMonths(-1).AddDays(1),
                    PaymentMethod = PaymentMethod.MPesa,
                    Status = PaymentStatus.Completed,
                    TransactionReference = "QHM7LP4X2N",
                    PaybillAccountNumber = "1A",
                    MPesaPhoneNumber = "+254716539952",
                    PeriodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-1),
                    PeriodEnd = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddMonths(-1).AddDays(1)
                },

                // Alice Wambui (Two-bedroom KSh 35,000) - M-Pesa Paybill
                new Payment
                {
                    TenantId = tenants[3].Id,
                    UnitId = kileleshwaUnits[0].Id,
                    LandlordAccountId = paymentAccounts[2].Id, // Kileleshwa Gardens Paybill
                    Amount = 35000,
                    PaymentDate = DateTime.UtcNow.AddMonths(-1).AddDays(2),
                    PaymentMethod = PaymentMethod.MPesa,
                    Status = PaymentStatus.Completed,
                    TransactionReference = "QHM9KP2X7N",
                    PaybillAccountNumber = "K-2A",
                    MPesaPhoneNumber = "+254745678901",
                    PeriodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-1),
                    PeriodEnd = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddMonths(-1).AddDays(2)
                },

                // Michael Kimani (Three-bedroom KSh 45,000) - M-Pesa Paybill
                new Payment
                {
                    TenantId = tenants[4].Id,
                    UnitId = kileleshwaUnits[2].Id,
                    LandlordAccountId = paymentAccounts[2].Id,
                    Amount = 45000,
                    PaymentDate = DateTime.UtcNow.AddDays(-5),
                    PaymentMethod = PaymentMethod.MPesa,
                    Status = PaymentStatus.Completed,
                    TransactionReference = "QHN7MP4K2X",
                    PaybillAccountNumber = "K-3A",
                    MPesaPhoneNumber = "+254756789012",
                    PeriodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1),
                    PeriodEnd = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1).AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },

                // Sarah Njeri (Luxury three-bedroom KSh 70,000) - Bank Transfer
                new Payment
                {
                    TenantId = tenants[5].Id,
                    UnitId = lavingtonUnits[1].Id,
                    LandlordAccountId = paymentAccounts[3].Id, // Lavington Court KCB
                    Amount = 70000,
                    PaymentDate = DateTime.UtcNow.AddDays(-2),
                    PaymentMethod = PaymentMethod.BankTransfer,
                    Status = PaymentStatus.Completed,
                    TransactionReference = "BANK20241202001",
                    PeriodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1),
                    PeriodEnd = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1).AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },

                // Daniel Otieno (Maisonette KSh 40,000) - M-Pesa Paybill
                new Payment
                {
                    TenantId = tenants[6].Id,
                    UnitId = utawalaUnits[0].Id,
                    LandlordAccountId = paymentAccounts[4].Id, // David Kamau M-Pesa
                    Amount = 40000,
                    PaymentDate = DateTime.UtcNow.AddMonths(-1).AddDays(5),
                    PaymentMethod = PaymentMethod.MPesa,
                    Status = PaymentStatus.Completed,
                    TransactionReference = "QHP9NX2M4L",
                    PaybillAccountNumber = "M1",
                    MPesaPhoneNumber = "+254778901234",
                    PeriodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-1),
                    PeriodEnd = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddMonths(-1).AddDays(5)
                },

                // Lucy Wanjiru (Bungalow KSh 30,000) - M-Pesa Paybill
                new Payment
                {
                    TenantId = tenants[7].Id,
                    UnitId = ruiruUnits[0].Id,
                    LandlordAccountId = paymentAccounts[4].Id,
                    Amount = 30000,
                    PaymentDate = DateTime.UtcNow.AddMonths(-1).AddDays(7),
                    PaymentMethod = PaymentMethod.MPesa,
                    Status = PaymentStatus.Completed,
                    TransactionReference = "QHK4MX7N2P",
                    PaybillAccountNumber = "BG1",
                    MPesaPhoneNumber = "+254789012345",
                    PeriodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-1),
                    PeriodEnd = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddMonths(-1).AddDays(7)
                },

                // Joseph Mutua (Bungalow KSh 35,000) - M-Pesa Paybill
                new Payment
                {
                    TenantId = tenants[8].Id,
                    UnitId = ruiruUnits[2].Id,
                    LandlordAccountId = paymentAccounts[4].Id,
                    Amount = 35000,
                    PaymentDate = DateTime.UtcNow.AddDays(-3),
                    PaymentMethod = PaymentMethod.MPesa,
                    Status = PaymentStatus.Completed,
                    TransactionReference = "QHN4MX7P9K",
                    PaybillAccountNumber = "BG3",
                    MPesaPhoneNumber = "+254790123456",
                    PeriodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1),
                    PeriodEnd = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1).AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
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
