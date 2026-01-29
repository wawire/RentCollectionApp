using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;
using RentCollection.IntegrationTests.Infrastructure;
using Xunit;

namespace RentCollection.IntegrationTests.Invoices;

public class InvoiceGenerationTests : IClassFixture<TestWebApplicationFactory>
{
    private const string JwtSecret = "test-secret-please-change-in-tests-only-1234567890";
    private readonly TestWebApplicationFactory _factory;

    public InvoiceGenerationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GenerateInvoices_Is_Idempotent_For_Same_Period()
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
            Id = 1,
            FirstName = "L",
            LastName = "L",
            Email = "landlord@test.com",
            Role = UserRole.Landlord,
            OrganizationId = organization.Id
        };
        var property = new Property
        {
            Id = 1,
            Name = "P1",
            Location = "Nairobi",
            TotalUnits = 1,
            LandlordId = landlord.Id,
            OrganizationId = organization.Id
        };
        var unit = new Unit
        {
            Id = 1,
            PropertyId = property.Id,
            Property = property,
            UnitNumber = "A1"
        };
        var tenant = new Tenant
        {
            Id = 1,
            UnitId = unit.Id,
            Unit = unit,
            FirstName = "T",
            LastName = "X",
            Status = TenantStatus.Active,
            MonthlyRent = 1000,
            RentDueDay = 5
        };

        context.Users.Add(landlord);
        context.Properties.Add(property);
        context.Units.Add(unit);
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var token = TestJwtFactory.CreateToken(
            JwtSecret,
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, UserRole.PlatformAdmin.ToString()));

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response1 = await client.PostAsync("/api/invoices/generate?year=2025&month=12", null);
        var response2 = await client.PostAsync("/api/invoices/generate?year=2025&month=12", null);

        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        (await context.Invoices.CountAsync()).Should().Be(1);
    }
}
