using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;
using RentCollection.IntegrationTests.Infrastructure;
using Xunit;

namespace RentCollection.IntegrationTests.Rbac;

public class RbacAccessTests : IClassFixture<TestWebApplicationFactory>
{
    private const string JwtSecret = "test-secret-please-change-in-tests-only-1234567890";
    private readonly TestWebApplicationFactory _factory;

    public RbacAccessTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Manager_Cannot_Access_Unassigned_Property()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var landlord = new User { Id = 1, FirstName = "L", LastName = "L", Email = "l@l.com", Role = UserRole.Landlord, OrganizationId = 1 };
        var property1 = new Property { Id = 1, Name = "Property-1", LandlordId = landlord.Id, Landlord = landlord, OrganizationId = 1 };
        var property2 = new Property { Id = 2, Name = "Property-2", LandlordId = landlord.Id, Landlord = landlord, OrganizationId = 1 };
        var manager = new User { Id = 10, FirstName = "M", LastName = "M", Email = "m@m.com", Role = UserRole.Manager, OrganizationId = 1 };

        context.Users.AddRange(landlord, manager);
        context.Properties.AddRange(property1, property2);
        context.UserPropertyAssignments.Add(new UserPropertyAssignment
        {
            UserId = manager.Id,
            PropertyId = property1.Id,
            AssignmentRole = UserRole.Manager,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var client = _factory.CreateClient();
        var token = TestJwtFactory.CreateToken(
            JwtSecret,
            new Claim(ClaimTypes.NameIdentifier, manager.Id.ToString()),
            new Claim(ClaimTypes.Role, UserRole.Manager.ToString()),
            new Claim("OrganizationId", "1"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/properties/{property2.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Caretaker_Cannot_Record_Payment()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var landlord = new User { Id = 4, FirstName = "Landlord", LastName = "Owner", Email = "owner@rentpro.com", Role = UserRole.Landlord, OrganizationId = 1 };
        var property = new Property { Id = 14, Name = "Operations Estate", LandlordId = landlord.Id, Landlord = landlord, OrganizationId = 1 };
        var unit = new Unit { Id = 24, PropertyId = property.Id, Property = property, UnitNumber = "101" };
        var tenant = new Tenant { Id = 34, UnitId = unit.Id, Unit = unit, FirstName = "Tenant", LastName = "Ops", Status = TenantStatus.Active, IsActive = true };
        var caretaker = new User { Id = 40, FirstName = "Cathy", LastName = "Caretaker", Email = "caretaker@rentpro.com", Role = UserRole.Caretaker, OrganizationId = 1 };

        context.Users.AddRange(landlord, caretaker);
        context.Properties.Add(property);
        context.Units.Add(unit);
        context.Tenants.Add(tenant);
        context.UserPropertyAssignments.Add(new UserPropertyAssignment
        {
            UserId = caretaker.Id,
            PropertyId = property.Id,
            AssignmentRole = UserRole.Caretaker,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var client = _factory.CreateClient();
        var token = TestJwtFactory.CreateToken(
            JwtSecret,
            new Claim(ClaimTypes.NameIdentifier, caretaker.Id.ToString()),
            new Claim(ClaimTypes.Role, UserRole.Caretaker.ToString()),
            new Claim("OrganizationId", "1"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new
        {
            TenantId = tenant.Id,
            Amount = 1500m,
            PaymentDate = DateTime.UtcNow,
            PeriodStart = DateTime.UtcNow.AddMonths(-1),
            PeriodEnd = DateTime.UtcNow,
            PaymentMethod = PaymentMethod.MPesa
        };

        var response = await client.PostAsJsonAsync("/api/payments", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Accountant_Can_Resolve_Unmatched_Payment()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var landlord = new User { Id = 5, FirstName = "Landa", LastName = "Keeper", Email = "landlord@rentpro.com", Role = UserRole.Landlord, OrganizationId = 1 };
        var property = new Property { Id = 15, Name = "Finance Tower", LandlordId = landlord.Id, Landlord = landlord, OrganizationId = 1 };
        var unit = new Unit { Id = 25, PropertyId = property.Id, Property = property, UnitNumber = "201" };
        var tenant = new Tenant { Id = 35, UnitId = unit.Id, Unit = unit, FirstName = "Tenant", LastName = "Finance", Status = TenantStatus.Active, IsActive = true };
        var accountant = new User { Id = 60, FirstName = "Anna", LastName = "Accountant", Email = "accountant@rentpro.com", Role = UserRole.Accountant, OrganizationId = 1 };
        var paymentAccount = new LandlordPaymentAccount
        {
            Id = 70,
            LandlordId = landlord.Id,
            PropertyId = property.Id,
            AccountName = "Finance Paybill",
            AccountType = PaymentAccountType.MPesaPaybill,
            PaybillNumber = "123456",
            IsDefault = true,
            IsActive = true
        };
        var unmatched = new UnmatchedPayment
        {
            Id = 80,
            TransactionReference = "UNM-123",
            Amount = 3200m,
            AccountReference = "ACCREF",
            PropertyId = property.Id,
            LandlordId = landlord.Id,
            Status = UnmatchedPaymentStatus.Pending
        };

        context.Users.AddRange(landlord, accountant);
        context.Properties.Add(property);
        context.Units.Add(unit);
        context.Tenants.Add(tenant);
        context.LandlordPaymentAccounts.Add(paymentAccount);
        context.UnmatchedPayments.Add(unmatched);
        context.UserPropertyAssignments.Add(new UserPropertyAssignment
        {
            UserId = accountant.Id,
            PropertyId = property.Id,
            AssignmentRole = UserRole.Accountant,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var client = _factory.CreateClient();
        var token = TestJwtFactory.CreateToken(
            JwtSecret,
            new Claim(ClaimTypes.NameIdentifier, accountant.Id.ToString()),
            new Claim(ClaimTypes.Role, UserRole.Accountant.ToString()),
            new Claim("OrganizationId", "1"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new
        {
            TenantId = tenant.Id,
            LandlordAccountId = paymentAccount.Id,
            PeriodStart = DateTime.UtcNow.AddMonths(-1),
            PeriodEnd = DateTime.UtcNow,
            PaymentDate = DateTime.UtcNow,
            Notes = "Accountant resolve"
        };

        var response = await client.PostAsJsonAsync($"/api/unmatchedpayments/{unmatched.Id}/resolve", payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await context.Entry(unmatched).ReloadAsync();
        unmatched.Status.Should().Be(UnmatchedPaymentStatus.Resolved);
    }

    [Fact]
    public async Task Accountant_Export_Returns_Assigned_Property_Rows_Only()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var landlord = new User { Id = 2, FirstName = "L", LastName = "L", Email = "l2@l.com", Role = UserRole.Landlord, OrganizationId = 1 };
        var property1 = new Property { Id = 11, Name = "Property-A", LandlordId = landlord.Id, Landlord = landlord, OrganizationId = 1 };
        var property2 = new Property { Id = 12, Name = "Property-B", LandlordId = landlord.Id, Landlord = landlord, OrganizationId = 1 };
        var unit1 = new Unit { Id = 21, PropertyId = property1.Id, Property = property1, UnitNumber = "A1" };
        var unit2 = new Unit { Id = 22, PropertyId = property2.Id, Property = property2, UnitNumber = "B1" };
        var tenant1 = new Tenant { Id = 31, UnitId = unit1.Id, Unit = unit1, FirstName = "T1", LastName = "X", Status = TenantStatus.Active, IsActive = true };
        var tenant2 = new Tenant { Id = 32, UnitId = unit2.Id, Unit = unit2, FirstName = "T2", LastName = "Y", Status = TenantStatus.Active, IsActive = true };
        var payment1 = new Payment
        {
            Id = 41,
            TenantId = tenant1.Id,
            Tenant = tenant1,
            UnitId = unit1.Id,
            Unit = unit1,
            Amount = 1000m,
            PaymentDate = DateTime.UtcNow.AddDays(-2),
            PaymentMethod = PaymentMethod.MPesa,
            Status = PaymentStatus.Completed
        };
        var payment2 = new Payment
        {
            Id = 42,
            TenantId = tenant2.Id,
            Tenant = tenant2,
            UnitId = unit2.Id,
            Unit = unit2,
            Amount = 2000m,
            PaymentDate = DateTime.UtcNow.AddDays(-1),
            PaymentMethod = PaymentMethod.MPesa,
            Status = PaymentStatus.Completed
        };
        var accountant = new User { Id = 20, FirstName = "A", LastName = "A", Email = "a@a.com", Role = UserRole.Accountant, OrganizationId = 1 };

        context.Users.AddRange(landlord, accountant);
        context.Properties.AddRange(property1, property2);
        context.Units.AddRange(unit1, unit2);
        context.Tenants.AddRange(tenant1, tenant2);
        context.Payments.AddRange(payment1, payment2);
        context.UserPropertyAssignments.Add(new UserPropertyAssignment
        {
            UserId = accountant.Id,
            PropertyId = property1.Id,
            AssignmentRole = UserRole.Accountant,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var client = _factory.CreateClient();
        var token = TestJwtFactory.CreateToken(
            JwtSecret,
            new Claim(ClaimTypes.NameIdentifier, accountant.Id.ToString()),
            new Claim(ClaimTypes.Role, UserRole.Accountant.ToString()),
            new Claim("OrganizationId", "1"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/exports/payments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var csv = await response.Content.ReadAsStringAsync();
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Length.Should().BeGreaterThan(1);

        foreach (var line in lines.Skip(1))
        {
            var columns = line.Split(',');
            columns[1].Should().Be("Property-A");
        }
    }

    [Fact]
    public async Task Accountant_Cannot_Create_Expense()
    {
        var client = _factory.CreateClient();
        var token = TestJwtFactory.CreateToken(
            JwtSecret,
            new Claim(ClaimTypes.NameIdentifier, "99"),
            new Claim(ClaimTypes.Role, UserRole.Accountant.ToString()),
            new Claim("OrganizationId", "1"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new
        {
            PropertyId = 1,
            Category = ExpenseCategory.Maintenance,
            Amount = 1000m,
            ExpenseDate = DateTime.UtcNow,
            Vendor = "Vendor",
            Description = "Test expense"
        };

        var response = await client.PostAsJsonAsync("/api/expenses", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
