using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;
using RentCollection.Infrastructure.Services;
using Xunit;

namespace RentCollection.UnitTests.Utilities;

public class UtilityBillingServiceTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task BuildLineItemsForTenantAsync_Calculates_Metered_Amount()
    {
        using var context = CreateDbContext();

        var landlord = new User { Id = 10, FirstName = "L", LastName = "L", Email = "l@l.com", OrganizationId = 1 };
        var property = new Property { Id = 1, Name = "P1", LandlordId = landlord.Id, Landlord = landlord, OrganizationId = 1 };
        var unit = new Unit { Id = 1, PropertyId = property.Id, Property = property, UnitNumber = "A1" };
        var tenant = new Tenant
        {
            Id = 1,
            UnitId = unit.Id,
            Unit = unit,
            FirstName = "T",
            LastName = "X",
            MonthlyRent = 1000,
            Status = TenantStatus.Active
        };

        var utilityType = new UtilityType
        {
            Id = 1,
            Name = "Water",
            BillingMode = UtilityBillingMode.Metered,
            UnitOfMeasure = "m3"
        };

        var config = new UtilityConfig
        {
            Id = 1,
            UtilityTypeId = utilityType.Id,
            UtilityType = utilityType,
            PropertyId = property.Id,
            Property = property,
            UnitId = unit.Id,
            Unit = unit,
            BillingMode = UtilityBillingMode.Metered,
            Rate = 10,
            EffectiveFrom = new DateTime(2024, 12, 1)
        };

        var previousReading = new MeterReading
        {
            UtilityConfigId = config.Id,
            UnitId = unit.Id,
            ReadingDate = new DateTime(2025, 1, 1),
            ReadingValue = 100
        };

        var currentReading = new MeterReading
        {
            UtilityConfigId = config.Id,
            UnitId = unit.Id,
            ReadingDate = new DateTime(2025, 1, 31),
            ReadingValue = 120
        };

        context.Users.Add(landlord);
        context.Properties.Add(property);
        context.Units.Add(unit);
        context.Tenants.Add(tenant);
        context.UtilityTypes.Add(utilityType);
        context.UtilityConfigs.Add(config);
        context.MeterReadings.AddRange(previousReading, currentReading);
        await context.SaveChangesAsync();

        var service = new UtilityBillingService(context, Mock.Of<ILogger<UtilityBillingService>>());
        var lineItems = await service.BuildLineItemsForTenantAsync(tenant, new DateTime(2025, 1, 1), new DateTime(2025, 1, 31));

        lineItems.Should().HaveCount(1);
        lineItems[0].Amount.Should().Be(200);
        lineItems[0].Quantity.Should().Be(20);
        lineItems[0].Rate.Should().Be(10);
        lineItems[0].Description.Should().Contain("Water");
    }
}
