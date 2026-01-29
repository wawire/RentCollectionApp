using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;
using RentCollection.IntegrationTests.Infrastructure;
using Xunit;

namespace RentCollection.IntegrationTests.Security;

public class TenantIdorTests : IClassFixture<TestWebApplicationFactory>
{
    private const string JwtSecret = "test-secret-please-change-in-tests-only-1234567890";
    private readonly TestWebApplicationFactory _factory;

    public TenantIdorTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Tenant_Cannot_Access_Other_Tenant_Document()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var organization = new Organization { Id = 1, Name = "Org", Status = OrganizationStatus.Active };
        var landlord = new User
        {
            Id = 1,
            FirstName = "Landlord",
            LastName = "Owner",
            Email = "landlord@hisa.local",
            PhoneNumber = "+254700000001",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("HisaRentals@2025"),
            Role = UserRole.Landlord,
            Status = UserStatus.Active,
            IsVerified = true,
            OrganizationId = organization.Id
        };

        var property = new Property
        {
            Id = 1,
            Name = "Property",
            LandlordId = landlord.Id,
            Landlord = landlord,
            OrganizationId = organization.Id
        };
        var unitA = new Unit { Id = 1, PropertyId = property.Id, Property = property, UnitNumber = "A1" };
        var unitB = new Unit { Id = 2, PropertyId = property.Id, Property = property, UnitNumber = "B1" };

        var tenantA = new Tenant { Id = 10, FirstName = "Tenant", LastName = "A", UnitId = unitA.Id, Unit = unitA, Status = TenantStatus.Active };
        var tenantB = new Tenant { Id = 11, FirstName = "Tenant", LastName = "B", UnitId = unitB.Id, Unit = unitB, Status = TenantStatus.Active };

        var userA = new User
        {
            Id = 100,
            FirstName = "Tenant",
            LastName = "A",
            Email = "tenant-a@hisa.local",
            PhoneNumber = "+254700000010",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("HisaRentalsTenant@2025"),
            Role = UserRole.Tenant,
            Status = UserStatus.Active,
            IsVerified = true,
            TenantId = tenantA.Id,
            OrganizationId = organization.Id
        };
        var userB = new User
        {
            Id = 101,
            FirstName = "Tenant",
            LastName = "B",
            Email = "tenant-b@hisa.local",
            PhoneNumber = "+254700000011",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("HisaRentalsTenant@2025"),
            Role = UserRole.Tenant,
            Status = UserStatus.Active,
            IsVerified = true,
            TenantId = tenantB.Id,
            OrganizationId = organization.Id
        };

        var document = new Document
        {
            Id = 200,
            DocumentType = DocumentType.LeaseAgreement,
            TenantId = tenantB.Id,
            FileName = "lease.pdf",
            FileUrl = "documents/lease.pdf",
            FileSize = 1024,
            ContentType = "application/pdf",
            UploadedByUserId = landlord.Id,
            UploadedBy = landlord
        };

        context.Organizations.Add(organization);
        context.Users.AddRange(landlord, userA, userB);
        context.Properties.Add(property);
        context.Units.AddRange(unitA, unitB);
        context.Tenants.AddRange(tenantA, tenantB);
        context.Documents.Add(document);
        await context.SaveChangesAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateTenantToken(userA));

        var response = await client.GetAsync($"/api/documents/{document.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Tenant_Cannot_Access_Other_Tenant_Invoice()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var organization = new Organization { Id = 1, Name = "Org", Status = OrganizationStatus.Active };
        var landlord = new User
        {
            Id = 1,
            FirstName = "Landlord",
            LastName = "Owner",
            Email = "landlord@hisa.local",
            PhoneNumber = "+254700000001",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("HisaRentals@2025"),
            Role = UserRole.Landlord,
            Status = UserStatus.Active,
            IsVerified = true,
            OrganizationId = organization.Id
        };

        var property = new Property
        {
            Id = 1,
            Name = "Property",
            LandlordId = landlord.Id,
            Landlord = landlord,
            OrganizationId = organization.Id
        };
        var unitA = new Unit { Id = 1, PropertyId = property.Id, Property = property, UnitNumber = "A1" };
        var unitB = new Unit { Id = 2, PropertyId = property.Id, Property = property, UnitNumber = "B1" };

        var tenantA = new Tenant { Id = 10, FirstName = "Tenant", LastName = "A", UnitId = unitA.Id, Unit = unitA, Status = TenantStatus.Active };
        var tenantB = new Tenant { Id = 11, FirstName = "Tenant", LastName = "B", UnitId = unitB.Id, Unit = unitB, Status = TenantStatus.Active };

        var userA = new User
        {
            Id = 100,
            FirstName = "Tenant",
            LastName = "A",
            Email = "tenant-a@hisa.local",
            PhoneNumber = "+254700000010",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("HisaRentalsTenant@2025"),
            Role = UserRole.Tenant,
            Status = UserStatus.Active,
            IsVerified = true,
            TenantId = tenantA.Id,
            OrganizationId = organization.Id
        };

        var invoice = new Invoice
        {
            Id = 300,
            TenantId = tenantB.Id,
            Tenant = tenantB,
            UnitId = unitB.Id,
            Unit = unitB,
            PropertyId = property.Id,
            Property = property,
            LandlordId = landlord.Id,
            Landlord = landlord,
            PeriodStart = DateTime.UtcNow.AddMonths(-1),
            PeriodEnd = DateTime.UtcNow.AddDays(-1),
            DueDate = DateTime.UtcNow.AddDays(5),
            Amount = 10000m,
            OpeningBalance = 0m,
            Balance = 10000m,
            Status = InvoiceStatus.Issued,
            LineItems = new List<InvoiceLineItem>
            {
                new InvoiceLineItem
                {
                    LineItemType = InvoiceLineItemType.Rent,
                    Description = "Rent",
                    Quantity = 1,
                    Rate = 10000m,
                    Amount = 10000m
                }
            }
        };

        context.Organizations.Add(organization);
        context.Users.AddRange(landlord, userA);
        context.Properties.Add(property);
        context.Units.AddRange(unitA, unitB);
        context.Tenants.AddRange(tenantA, tenantB);
        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateTenantToken(userA));

        var response = await client.GetAsync($"/api/invoices/{invoice.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static string CreateTenantToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("OrganizationId", user.OrganizationId.ToString()),
            new Claim("TenantId", user.TenantId?.ToString() ?? string.Empty)
        };

        return TestJwtFactory.CreateToken(JwtSecret, claims.ToArray());
    }
}
