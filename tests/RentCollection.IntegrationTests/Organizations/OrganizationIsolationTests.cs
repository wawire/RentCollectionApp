using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RentCollection.Application.DTOs.Organizations;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;
using RentCollection.IntegrationTests.Infrastructure;
using Xunit;

namespace RentCollection.IntegrationTests.Organizations;

public class OrganizationIsolationTests : IClassFixture<TestWebApplicationFactory>
{
    private const string JwtSecret = "test-secret-please-change-in-tests-only-1234567890";
    private readonly TestWebApplicationFactory _factory;

    public OrganizationIsolationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task User_From_Org_A_Cannot_Get_Org_B_Property()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var orgA = new Organization { Name = "Org A", CreatedAt = DateTime.UtcNow };
        var orgB = new Organization { Name = "Org B", CreatedAt = DateTime.UtcNow };

        var managerA = new User
        {
            Id = 100,
            FirstName = "M",
            LastName = "A",
            Email = "manager-a@test.com",
            Role = UserRole.Manager,
            OrganizationId = 1
        };

        context.Organizations.AddRange(orgA, orgB);
        await context.SaveChangesAsync();

        managerA.OrganizationId = orgA.Id;

        var propertyB = new Property
        {
            Id = 200,
            Name = "Org B Property",
            Location = "Nairobi",
            TotalUnits = 1,
            OrganizationId = orgB.Id
        };

        context.Users.Add(managerA);
        context.Properties.Add(propertyB);
        context.UserPropertyAssignments.Add(new UserPropertyAssignment
        {
            UserId = managerA.Id,
            PropertyId = propertyB.Id,
            AssignmentRole = UserRole.Manager,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var token = TestJwtFactory.CreateToken(
            JwtSecret,
            new Claim(ClaimTypes.NameIdentifier, managerA.Id.ToString()),
            new Claim(ClaimTypes.Role, UserRole.Manager.ToString()),
            new Claim("OrganizationId", orgA.Id.ToString()));

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/properties/{propertyB.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PlatformAdmin_Creates_Org_And_Assigns_Manager_To_Property()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var systemAdminToken = TestJwtFactory.CreateToken(
            JwtSecret,
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, UserRole.PlatformAdmin.ToString()));

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", systemAdminToken);

        var organization = await client.PostAsJsonAsync("/api/organizations", new CreateOrganizationDto
        {
            Name = "New Org"
        });

        organization.StatusCode.Should().Be(HttpStatusCode.Created);
        var orgDto = await organization.Content.ReadFromJsonAsync<OrganizationDto>();
        orgDto.Should().NotBeNull();

        var propertyResponse = await client.PostAsJsonAsync("/api/properties", new
        {
            Name = "Org Property",
            Location = "Nairobi",
            Description = "Org property",
            TotalUnits = 2,
            OrganizationId = orgDto!.Id
        });

        propertyResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var propertyJson = await propertyResponse.Content.ReadFromJsonAsync<JsonElement>();
        var propertyId = propertyJson.GetProperty("data").GetProperty("id").GetInt32();

        var createUserResponse = await client.PostAsJsonAsync($"/api/organizations/{orgDto.Id}/users", new
        {
            FirstName = "Manager",
            LastName = "One",
            Email = "manager.one@test.com",
            PhoneNumber = "0712345678",
            Password = "Password@123",
            ConfirmPassword = "Password@123",
            Role = UserRole.Manager
        });

        createUserResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdUserJson = await createUserResponse.Content.ReadFromJsonAsync<JsonElement>();
        var createdUserId = createdUserJson.GetProperty("userId").GetInt32();

        var assignResponse = await client.PostAsJsonAsync(
            $"/api/organizations/{orgDto.Id}/properties/{propertyId}/assign-user",
            new
            {
                UserId = createdUserId,
                AssignmentRole = UserRole.Manager
            });

        assignResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var otherOrgResponse = await client.PostAsJsonAsync("/api/organizations", new CreateOrganizationDto
        {
            Name = "Other Org"
        });

        otherOrgResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var otherOrg = await otherOrgResponse.Content.ReadFromJsonAsync<OrganizationDto>();

        var otherPropertyResponse = await client.PostAsJsonAsync("/api/properties", new
        {
            Name = "Other Property",
            Location = "Mombasa",
            Description = "Other org property",
            TotalUnits = 1,
            OrganizationId = otherOrg!.Id
        });

        otherPropertyResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var otherPropertyJson = await otherPropertyResponse.Content.ReadFromJsonAsync<JsonElement>();
        var otherPropertyId = otherPropertyJson.GetProperty("data").GetProperty("id").GetInt32();

        var managerToken = TestJwtFactory.CreateToken(
            JwtSecret,
            new Claim(ClaimTypes.NameIdentifier, createdUserId.ToString()),
            new Claim(ClaimTypes.Role, UserRole.Manager.ToString()),
            new Claim("OrganizationId", orgDto.Id.ToString()));

        var managerClient = _factory.CreateClient();
        managerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);

        var propertyResult = await managerClient.GetAsync($"/api/properties/{propertyId}");
        propertyResult.StatusCode.Should().Be(HttpStatusCode.OK);

        var otherPropertyResult = await managerClient.GetAsync($"/api/properties/{otherPropertyId}");
        otherPropertyResult.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
