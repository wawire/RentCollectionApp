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

namespace RentCollection.IntegrationTests.Documents;

public class DocumentIsolationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public DocumentIsolationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Tenant_Cannot_Access_Other_Tenant_Documents()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var uploader = new User { Id = 10, FirstName = "U", LastName = "L", Email = "u@l.com", OrganizationId = 1 };
        var landlord = new User { Id = 1, FirstName = "L", LastName = "L", Email = "l@l.com", OrganizationId = 1 };
        var property = new Property { Id = 1, Name = "P1", LandlordId = landlord.Id, Landlord = landlord, OrganizationId = 1 };
        var unitA = new Unit { Id = 1, PropertyId = property.Id, Property = property, UnitNumber = "A1" };
        var unitB = new Unit { Id = 2, PropertyId = property.Id, Property = property, UnitNumber = "B1" };
        var tenantA = new Tenant { Id = 1, FirstName = "A", LastName = "T", Email = "a@t.com", UnitId = unitA.Id, Unit = unitA };
        var tenantB = new Tenant { Id = 2, FirstName = "B", LastName = "T", Email = "b@t.com", UnitId = unitB.Id, Unit = unitB };

        context.Users.AddRange(landlord, uploader);
        context.Properties.Add(property);
        context.Units.AddRange(unitA, unitB);
        context.Tenants.AddRange(tenantA, tenantB);
        context.Documents.Add(new Document
        {
            Id = 1,
            DocumentType = DocumentType.LeaseAgreement,
            TenantId = tenantB.Id,
            FileName = "lease.pdf",
            FileUrl = "/uploads/lease.pdf",
            FileSize = 10,
            ContentType = "application/pdf",
            UploadedByUserId = uploader.Id
        });

        await context.SaveChangesAsync();

        var token = TestJwtFactory.CreateToken(
            "test-secret-please-change-in-tests-only-1234567890",
            new Claim(ClaimTypes.NameIdentifier, "1001"),
            new Claim(ClaimTypes.Role, "Tenant"),
            new Claim("TenantId", tenantA.Id.ToString()),
            new Claim("OrganizationId", "1"));

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/documents/tenant/{tenantB.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
