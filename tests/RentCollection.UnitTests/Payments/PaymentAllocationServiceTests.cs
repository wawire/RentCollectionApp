using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;
using RentCollection.Infrastructure.Services;
using RentCollection.UnitTests.TestDoubles;
using Xunit;

namespace RentCollection.UnitTests.Payments;

public class PaymentAllocationServiceTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AllocatePaymentAsync_Allocates_Partial_And_Leaves_Unallocated()
    {
        using var context = CreateDbContext();

        var landlord = new User { Id = 1, FirstName = "L", LastName = "L", Email = "l@l.com", OrganizationId = 1 };
        var property = new Property { Id = 1, Name = "P1", LandlordId = landlord.Id, Landlord = landlord, OrganizationId = 1 };
        var unit = new Unit { Id = 1, PropertyId = property.Id, Property = property, UnitNumber = "A1" };
        var tenant = new Tenant { Id = 1, UnitId = unit.Id, Unit = unit, FirstName = "T", LastName = "X", Status = TenantStatus.Active };
        var invoice = new Invoice
        {
            Id = 1,
            TenantId = tenant.Id,
            Tenant = tenant,
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
            Balance = 1000,
            Status = InvoiceStatus.Issued
        };

        var payment = new Payment
        {
            Id = 1,
            TenantId = tenant.Id,
            Tenant = tenant,
            UnitId = unit.Id,
            Unit = unit,
            LandlordAccountId = 1,
            Amount = 1500,
            UnallocatedAmount = 1500,
            PaymentDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow,
            PaymentMethod = PaymentMethod.Cash,
            Status = PaymentStatus.Completed,
            PeriodStart = new DateTime(2025, 12, 1),
            PeriodEnd = new DateTime(2025, 12, 31)
        };

        context.Users.Add(landlord);
        context.Properties.Add(property);
        context.Units.Add(unit);
        context.Tenants.Add(tenant);
        context.Invoices.Add(invoice);
        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService { IsPlatformAdmin = true };
        var service = new PaymentAllocationService(context, currentUser, Mock.Of<ILogger<PaymentAllocationService>>());

        var result = await service.AllocatePaymentAsync(payment.Id);

        result.IsSuccess.Should().BeTrue();
        var updatedInvoice = await context.Invoices.FirstAsync();
        updatedInvoice.Balance.Should().Be(0);
        updatedInvoice.Status.Should().Be(InvoiceStatus.Paid);

        var updatedPayment = await context.Payments.Include(p => p.Allocations).FirstAsync();
        updatedPayment.Allocations.Should().HaveCount(1);
        updatedPayment.UnallocatedAmount.Should().Be(500);
    }

    [Fact]
    public async Task AllocatePaymentAsync_Allocates_Across_Multiple_Invoices()
    {
        using var context = CreateDbContext();

        var landlord = new User { Id = 1, FirstName = "L", LastName = "L", Email = "l@l.com", OrganizationId = 1 };
        var property = new Property { Id = 1, Name = "P1", LandlordId = landlord.Id, Landlord = landlord, OrganizationId = 1 };
        var unit = new Unit { Id = 1, PropertyId = property.Id, Property = property, UnitNumber = "A1" };
        var tenant = new Tenant { Id = 1, UnitId = unit.Id, Unit = unit, FirstName = "T", LastName = "X", Status = TenantStatus.Active };

        var invoice1 = new Invoice
        {
            Id = 1,
            TenantId = tenant.Id,
            Tenant = tenant,
            UnitId = unit.Id,
            Unit = unit,
            PropertyId = property.Id,
            Property = property,
            LandlordId = landlord.Id,
            Landlord = landlord,
            PeriodStart = new DateTime(2025, 11, 1),
            PeriodEnd = new DateTime(2025, 11, 30),
            DueDate = new DateTime(2025, 11, 5),
            Amount = 600,
            OpeningBalance = 0,
            Balance = 600,
            Status = InvoiceStatus.Issued
        };
        var invoice2 = new Invoice
        {
            Id = 2,
            TenantId = tenant.Id,
            Tenant = tenant,
            UnitId = unit.Id,
            Unit = unit,
            PropertyId = property.Id,
            Property = property,
            LandlordId = landlord.Id,
            Landlord = landlord,
            PeriodStart = new DateTime(2025, 12, 1),
            PeriodEnd = new DateTime(2025, 12, 31),
            DueDate = new DateTime(2025, 12, 5),
            Amount = 300,
            OpeningBalance = 0,
            Balance = 300,
            Status = InvoiceStatus.Issued
        };

        var payment = new Payment
        {
            Id = 1,
            TenantId = tenant.Id,
            Tenant = tenant,
            UnitId = unit.Id,
            Unit = unit,
            LandlordAccountId = 1,
            Amount = 900,
            UnallocatedAmount = 900,
            PaymentDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow,
            PaymentMethod = PaymentMethod.Cash,
            Status = PaymentStatus.Completed,
            PeriodStart = new DateTime(2025, 12, 1),
            PeriodEnd = new DateTime(2025, 12, 31)
        };

        context.Users.Add(landlord);
        context.Properties.Add(property);
        context.Units.Add(unit);
        context.Tenants.Add(tenant);
        context.Invoices.AddRange(invoice1, invoice2);
        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService { IsPlatformAdmin = true };
        var service = new PaymentAllocationService(context, currentUser, Mock.Of<ILogger<PaymentAllocationService>>());

        var result = await service.AllocatePaymentAsync(payment.Id);

        result.IsSuccess.Should().BeTrue();
        var updatedInvoices = await context.Invoices.OrderBy(i => i.Id).ToListAsync();
        updatedInvoices[0].Balance.Should().Be(0);
        updatedInvoices[0].Status.Should().Be(InvoiceStatus.Paid);
        updatedInvoices[1].Balance.Should().Be(0);
        updatedInvoices[1].Status.Should().Be(InvoiceStatus.Paid);

        var updatedPayment = await context.Payments.Include(p => p.Allocations).FirstAsync();
        updatedPayment.Allocations.Should().HaveCount(2);
        updatedPayment.UnallocatedAmount.Should().Be(0);
    }
}
