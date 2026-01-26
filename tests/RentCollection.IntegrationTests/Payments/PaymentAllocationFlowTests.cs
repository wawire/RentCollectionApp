using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;
using RentCollection.Infrastructure.Services;
using RentCollection.IntegrationTests.Infrastructure;
using Xunit;

namespace RentCollection.IntegrationTests.Payments;

public class PaymentAllocationFlowTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public PaymentAllocationFlowTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AllocatePayment_Updates_Invoice_Status()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var tenant = await SeedTenantAsync(context);

        var invoice = new Invoice
        {
            TenantId = tenant.Id,
            UnitId = tenant.UnitId,
            PropertyId = tenant.Unit.PropertyId,
            LandlordId = tenant.Unit.Property.LandlordId!.Value,
            PeriodStart = new DateTime(2025, 12, 1),
            PeriodEnd = new DateTime(2025, 12, 31),
            DueDate = DateTime.UtcNow.Date.AddDays(5),
            Amount = 1000,
            OpeningBalance = 0,
            Balance = 1000,
            Status = InvoiceStatus.Issued
        };
        var payment = new Payment
        {
            TenantId = tenant.Id,
            UnitId = tenant.UnitId,
            LandlordAccountId = tenant.Unit.Property.PaymentAccounts.First().Id,
            Amount = 1000,
            UnallocatedAmount = 1000,
            PaymentDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow,
            PaymentMethod = PaymentMethod.Cash,
            Status = PaymentStatus.Completed,
            PeriodStart = invoice.PeriodStart,
            PeriodEnd = invoice.PeriodEnd
        };

        context.Invoices.Add(invoice);
        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var allocationService = new PaymentAllocationService(
            context,
            new TestCurrentUserService { IsPlatformAdmin = true },
            NullLogger<PaymentAllocationService>.Instance);

        var result = await allocationService.AllocatePaymentToOutstandingInvoicesAsync(payment.Id);

        result.IsSuccess.Should().BeTrue();

        var updatedInvoice = await context.Invoices.FirstAsync();
        updatedInvoice.Balance.Should().Be(0);
        updatedInvoice.Status.Should().Be(InvoiceStatus.Paid);
    }

    [Fact]
    public async Task ReverseAllocations_Rolls_Back_Invoice_Status()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var tenant = await SeedTenantAsync(context);

        var invoice = new Invoice
        {
            TenantId = tenant.Id,
            UnitId = tenant.UnitId,
            PropertyId = tenant.Unit.PropertyId,
            LandlordId = tenant.Unit.Property.LandlordId!.Value,
            PeriodStart = new DateTime(2025, 11, 1),
            PeriodEnd = new DateTime(2025, 11, 30),
            DueDate = DateTime.UtcNow.Date.AddDays(3),
            Amount = 800,
            OpeningBalance = 0,
            Balance = 800,
            Status = InvoiceStatus.Issued
        };
        var payment = new Payment
        {
            TenantId = tenant.Id,
            UnitId = tenant.UnitId,
            LandlordAccountId = tenant.Unit.Property.PaymentAccounts.First().Id,
            Amount = 800,
            UnallocatedAmount = 800,
            PaymentDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow,
            PaymentMethod = PaymentMethod.Cash,
            Status = PaymentStatus.Completed,
            PeriodStart = invoice.PeriodStart,
            PeriodEnd = invoice.PeriodEnd
        };

        context.Invoices.Add(invoice);
        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var allocationService = new PaymentAllocationService(
            context,
            new TestCurrentUserService { IsPlatformAdmin = true },
            NullLogger<PaymentAllocationService>.Instance);

        var allocateResult = await allocationService.AllocatePaymentToOutstandingInvoicesAsync(payment.Id);
        allocateResult.IsSuccess.Should().BeTrue();

        var reverseResult = await allocationService.ReverseAllocationsAsync(payment.Id, "Test reversal");
        reverseResult.IsSuccess.Should().BeTrue();

        var updatedInvoice = await context.Invoices.FirstAsync();
        updatedInvoice.Balance.Should().Be(800);
        updatedInvoice.Status.Should().Be(InvoiceStatus.Issued);

        var updatedPayment = await context.Payments.FirstAsync();
        updatedPayment.UnallocatedAmount.Should().Be(800);
    }

    [Fact]
    public async Task Overpayment_Creates_Unallocated_Balance()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var tenant = await SeedTenantAsync(context);

        var invoice = new Invoice
        {
            TenantId = tenant.Id,
            UnitId = tenant.UnitId,
            PropertyId = tenant.Unit.PropertyId,
            LandlordId = tenant.Unit.Property.LandlordId!.Value,
            PeriodStart = new DateTime(2025, 12, 1),
            PeriodEnd = new DateTime(2025, 12, 31),
            DueDate = DateTime.UtcNow.Date.AddDays(5),
            Amount = 500,
            OpeningBalance = 0,
            Balance = 500,
            Status = InvoiceStatus.Issued
        };
        var payment = new Payment
        {
            TenantId = tenant.Id,
            UnitId = tenant.UnitId,
            LandlordAccountId = tenant.Unit.Property.PaymentAccounts.First().Id,
            Amount = 800,
            UnallocatedAmount = 800,
            PaymentDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow,
            PaymentMethod = PaymentMethod.Cash,
            Status = PaymentStatus.Completed,
            PeriodStart = invoice.PeriodStart,
            PeriodEnd = invoice.PeriodEnd
        };

        context.Invoices.Add(invoice);
        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var allocationService = new PaymentAllocationService(
            context,
            new TestCurrentUserService { IsPlatformAdmin = true },
            NullLogger<PaymentAllocationService>.Instance);

        var result = await allocationService.AllocatePaymentToOutstandingInvoicesAsync(payment.Id);

        result.IsSuccess.Should().BeTrue();

        var updatedPayment = await context.Payments.FirstAsync();
        updatedPayment.UnallocatedAmount.Should().Be(300);
    }

    private static async Task<Tenant> SeedTenantAsync(ApplicationDbContext context)
    {
        var organization = new Organization { Id = 1, Name = "Org", CreatedAt = DateTime.UtcNow };
        var landlord = new User
        {
            Id = 10,
            FirstName = "L",
            LastName = "L",
            Email = "l@l.com",
            Role = UserRole.Landlord,
            OrganizationId = organization.Id,
            Organization = organization
        };
        var property = new Property
        {
            Id = 20,
            Name = "P1",
            Location = "Nairobi",
            TotalUnits = 1,
            LandlordId = landlord.Id,
            OrganizationId = organization.Id,
            Organization = organization
        };
        var unit = new Unit { Id = 30, PropertyId = property.Id, Property = property, UnitNumber = "A1" };
        var tenant = new Tenant { Id = 40, UnitId = unit.Id, Unit = unit, FirstName = "T", LastName = "X", Status = TenantStatus.Active, IsActive = true };
        var paymentAccount = new LandlordPaymentAccount
        {
            Id = 50,
            PropertyId = property.Id,
            Property = property,
            LandlordId = landlord.Id,
            AccountType = PaymentAccountType.MPesaPaybill,
            MPesaShortCode = "123456",
            IsActive = true
        };

        context.Organizations.Add(organization);
        context.Users.Add(landlord);
        context.Properties.Add(property);
        context.Units.Add(unit);
        context.Tenants.Add(tenant);
        context.LandlordPaymentAccounts.Add(paymentAccount);
        await context.SaveChangesAsync();

        await context.Entry(tenant).Reference(t => t.Unit).LoadAsync();
        await context.Entry(unit).Reference(u => u.Property).LoadAsync();
        await context.Entry(property).Collection(p => p.PaymentAccounts).LoadAsync();

        return tenant;
    }

    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public string? UserId => "1";
        public int? UserIdInt => 1;
        public string? Email => "test@local";
        public string? Role => UserRole.PlatformAdmin.ToString();
        public string? LandlordId => null;
        public int? LandlordIdInt => null;
        public int? OrganizationId => null;
        public int? TenantId => null;
        public int? PropertyId => null;
        public bool IsAuthenticated => true;
        public bool IsPlatformAdmin { get; init; }
        public bool IsLandlord => false;
        public bool IsCaretaker => false;
        public bool IsManager => false;
        public bool IsAccountant => false;
        public bool IsTenant => false;
        public Task<IReadOnlyCollection<int>> GetAssignedPropertyIdsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<int>>(Array.Empty<int>());
    }
}
