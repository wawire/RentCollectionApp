using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;
using RentCollection.Infrastructure.Services;
using RentCollection.UnitTests.TestDoubles;
using Xunit;

namespace RentCollection.UnitTests.Invoices;

public class InvoiceServiceTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GenerateMonthlyInvoicesAsync_Is_Idempotent()
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

        context.Users.Add(landlord);
        context.Properties.Add(property);
        context.Units.Add(unit);
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService { IsPlatformAdmin = true };
        var utilityBillingService = new UtilityBillingService(context, Mock.Of<ILogger<UtilityBillingService>>());
        var service = new InvoiceService(context, currentUser, utilityBillingService, Mock.Of<ILogger<InvoiceService>>());

        var result1 = await service.GenerateMonthlyInvoicesAsync(2025, 12);
        var result2 = await service.GenerateMonthlyInvoicesAsync(2025, 12);

        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        (await context.Invoices.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task GenerateMonthlyInvoicesAsync_Creates_Rent_LineItem()
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

        context.Users.Add(landlord);
        context.Properties.Add(property);
        context.Units.Add(unit);
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService { IsPlatformAdmin = true };
        var utilityBillingService = new UtilityBillingService(context, Mock.Of<ILogger<UtilityBillingService>>());
        var service = new InvoiceService(context, currentUser, utilityBillingService, Mock.Of<ILogger<InvoiceService>>());

        var result = await service.GenerateMonthlyInvoicesAsync(2025, 12);

        result.IsSuccess.Should().BeTrue();
        var invoice = await context.Invoices.Include(i => i.LineItems).FirstAsync();
        invoice.LineItems.Should().ContainSingle(item => item.LineItemType == InvoiceLineItemType.Rent);
    }

    [Fact]
    public async Task GetInvoiceByIdAsync_Fails_For_Other_Tenant()
    {
        using var context = CreateDbContext();

        var landlord = new User { Id = 10, FirstName = "L", LastName = "L", Email = "l@l.com", OrganizationId = 1 };
        var property = new Property { Id = 1, Name = "P1", LandlordId = landlord.Id, Landlord = landlord, OrganizationId = 1 };
        var unit = new Unit { Id = 1, PropertyId = property.Id, Property = property, UnitNumber = "A1" };
        var tenant1 = new Tenant
        {
            Id = 1,
            UnitId = unit.Id,
            Unit = unit,
            FirstName = "T1",
            LastName = "X",
            MonthlyRent = 1000,
            Status = TenantStatus.Active
        };
        var tenant2 = new Tenant
        {
            Id = 2,
            UnitId = unit.Id,
            Unit = unit,
            FirstName = "T2",
            LastName = "Y",
            MonthlyRent = 900,
            Status = TenantStatus.Active
        };

        var invoice = new Invoice
        {
            TenantId = tenant1.Id,
            Tenant = tenant1,
            UnitId = unit.Id,
            Unit = unit,
            PropertyId = property.Id,
            Property = property,
            LandlordId = landlord.Id,
            Landlord = landlord,
            PeriodStart = new DateTime(2025, 12, 1),
            PeriodEnd = new DateTime(2025, 12, 31),
            DueDate = new DateTime(2025, 12, 5),
            Amount = 1000,
            OpeningBalance = 0,
            Balance = 1000
        };

        context.Users.Add(landlord);
        context.Properties.Add(property);
        context.Units.Add(unit);
        context.Tenants.AddRange(tenant1, tenant2);
        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService
        {
            IsTenant = true,
            TenantId = tenant2.Id,
            OrganizationId = 1
        };
        var utilityBillingService = new UtilityBillingService(context, Mock.Of<ILogger<UtilityBillingService>>());
        var service = new InvoiceService(context, currentUser, utilityBillingService, Mock.Of<ILogger<InvoiceService>>());

        var result = await service.GetInvoiceByIdAsync(invoice.Id);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("You do not have permission to view this invoice");
    }
}
