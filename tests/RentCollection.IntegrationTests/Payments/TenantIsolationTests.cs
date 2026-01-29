using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;
using RentCollection.IntegrationTests.Infrastructure;
using Xunit;

namespace RentCollection.IntegrationTests.Payments;

public class TenantIsolationTests : IClassFixture<TestWebApplicationFactory>
{
    private const string JwtSecret = "test-secret-please-change-in-tests-only-1234567890";
    private readonly TestWebApplicationFactory _factory;

    public TenantIsolationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Tenant_Cannot_See_Other_Tenant_Payments()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var organization = new Organization { Name = "Org", CreatedAt = DateTime.UtcNow };
        context.Organizations.Add(organization);
        await context.SaveChangesAsync();

        var landlord = new User
        {
            Id = 10,
            FirstName = "L",
            LastName = "L",
            Email = "landlord@test.com",
            Role = UserRole.Landlord,
            OrganizationId = organization.Id
        };
        var property = new Property
        {
            Id = 20,
            Name = "P1",
            Location = "Nairobi",
            TotalUnits = 2,
            LandlordId = landlord.Id,
            OrganizationId = organization.Id
        };
        var unit1 = new Unit { Id = 30, PropertyId = property.Id, Property = property, UnitNumber = "A1" };
        var unit2 = new Unit { Id = 31, PropertyId = property.Id, Property = property, UnitNumber = "A2" };
        var tenant1 = new Tenant
        {
            Id = 40,
            UnitId = unit1.Id,
            Unit = unit1,
            FirstName = "T1",
            LastName = "X",
            Status = TenantStatus.Active,
            IsActive = true
        };
        var tenant2 = new Tenant
        {
            Id = 41,
            UnitId = unit2.Id,
            Unit = unit2,
            FirstName = "T2",
            LastName = "Y",
            Status = TenantStatus.Active,
            IsActive = true
        };
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

        var payment1 = new Payment
        {
            Id = 60,
            TenantId = tenant1.Id,
            UnitId = unit1.Id,
            LandlordAccountId = paymentAccount.Id,
            Amount = 1000,
            PaymentDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.Date,
            PaymentMethod = PaymentMethod.Cash,
            Status = PaymentStatus.Completed,
            PeriodStart = DateTime.UtcNow.Date.AddDays(-30),
            PeriodEnd = DateTime.UtcNow.Date,
            UnallocatedAmount = 0
        };
        var payment2 = new Payment
        {
            Id = 61,
            TenantId = tenant2.Id,
            UnitId = unit2.Id,
            LandlordAccountId = paymentAccount.Id,
            Amount = 1200,
            PaymentDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.Date,
            PaymentMethod = PaymentMethod.Cash,
            Status = PaymentStatus.Completed,
            PeriodStart = DateTime.UtcNow.Date.AddDays(-30),
            PeriodEnd = DateTime.UtcNow.Date,
            UnallocatedAmount = 0
        };

        context.Users.Add(landlord);
        context.Properties.Add(property);
        context.Units.AddRange(unit1, unit2);
        context.Tenants.AddRange(tenant1, tenant2);
        context.LandlordPaymentAccounts.Add(paymentAccount);
        context.Payments.AddRange(payment1, payment2);
        await context.SaveChangesAsync();

        var token = TestJwtFactory.CreateToken(
            JwtSecret,
            new Claim(ClaimTypes.NameIdentifier, "100"),
            new Claim(ClaimTypes.Role, UserRole.Tenant.ToString()),
            new Claim("TenantId", tenant1.Id.ToString()),
            new Claim("OrganizationId", organization.Id.ToString()));

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/tenantpayments/history");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.TryGetProperty("data", out var data).Should().BeTrue();
        data.ValueKind.Should().Be(JsonValueKind.Array);
        data.GetArrayLength().Should().Be(1);
        data[0].GetProperty("tenantId").GetInt32().Should().Be(tenant1.Id);
    }
}
