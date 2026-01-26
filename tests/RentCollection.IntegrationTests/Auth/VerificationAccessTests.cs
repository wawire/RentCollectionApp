using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;
using RentCollection.IntegrationTests.Infrastructure;
using Xunit;

namespace RentCollection.IntegrationTests.Auth;

public class VerificationAccessTests : IClassFixture<TestWebApplicationFactory>
{
    private const string JwtSecret = "test-secret-please-change-in-tests-only-1234567890";
    private readonly TestWebApplicationFactory _factory;

    public VerificationAccessTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UnverifiedUser_IsBlocked_FromPayments()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var organization = new Organization { Id = 1, Name = "Org", Status = OrganizationStatus.Active };
        var user = new User
        {
            Id = 10,
            FirstName = "Landlord",
            LastName = "User",
            Email = "landlord@hisa.local",
            PhoneNumber = "+254700000010",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("HisaRentals@2025"),
            Role = UserRole.Landlord,
            Status = UserStatus.Active,
            IsVerified = false,
            OrganizationId = organization.Id
        };

        context.Organizations.Add(organization);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(user));

        var response = await client.GetAsync("/api/payments");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task VerifiedUser_CanAccess_Payments()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var organization = new Organization { Id = 1, Name = "Org", Status = OrganizationStatus.Active };
        var user = new User
        {
            Id = 11,
            FirstName = "Landlord",
            LastName = "Verified",
            Email = "verified@hisa.local",
            PhoneNumber = "+254700000011",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("HisaRentals@2025"),
            Role = UserRole.Landlord,
            Status = UserStatus.Active,
            IsVerified = true,
            OrganizationId = organization.Id
        };

        context.Organizations.Add(organization);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(user));

        var response = await client.GetAsync("/api/payments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task TenantWithPasswordChangeRequired_IsBlockedFromTenantPortal()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var organization = new Organization { Id = 1, Name = "Org", Status = OrganizationStatus.Active };
        var tenant = new Tenant
        {
            Id = 21,
            FirstName = "Tenant",
            LastName = "User",
            Email = "tenant@hisa.local",
            PhoneNumber = "+254711111111",
            Status = TenantStatus.Active
        };
        var user = new User
        {
            Id = 20,
            FirstName = "Tenant",
            LastName = "User",
            Email = "tenant@hisa.local",
            PhoneNumber = "+254711111111",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("HisaRentalsTenant@2025"),
            Role = UserRole.Tenant,
            Status = UserStatus.Active,
            IsVerified = true,
            MustChangePassword = true,
            TenantId = tenant.Id,
            OrganizationId = organization.Id
        };

        context.Organizations.Add(organization);
        context.Tenants.Add(tenant);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(user));

        var response = await client.GetAsync("/api/tenantportal/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Login_RateLimit_LocksOutAfterMaxAttempts()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var organization = new Organization { Id = 1, Name = "Org", Status = OrganizationStatus.Active };
        var user = new User
        {
            Id = 30,
            FirstName = "Lock",
            LastName = "Out",
            Email = "lockout@hisa.local",
            PhoneNumber = "+254722222222",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("HisaRentals@2025"),
            Role = UserRole.Manager,
            Status = UserStatus.Active,
            IsVerified = true,
            OrganizationId = organization.Id
        };

        context.Organizations.Add(organization);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var client = _factory.CreateClient();

        var badLogin = new { emailOrPhone = user.Email, password = "WrongPassword1!" };

        var first = await client.PostAsJsonAsync("/api/auth/login", badLogin);
        first.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var second = await client.PostAsJsonAsync("/api/auth/login", badLogin);
        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var correctLogin = new { emailOrPhone = user.Email, password = "HisaRentals@2025" };
        var locked = await client.PostAsJsonAsync("/api/auth/login", correctLogin);
        locked.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("OrganizationId", user.OrganizationId.ToString())
        };

        if (user.TenantId.HasValue)
        {
            claims.Add(new Claim("TenantId", user.TenantId.Value.ToString()));
        }

        return TestJwtFactory.CreateToken(JwtSecret, claims.ToArray());
    }
}
