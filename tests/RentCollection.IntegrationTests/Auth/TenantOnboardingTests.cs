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

public class TenantOnboardingTests : IClassFixture<TestWebApplicationFactory>
{
    private const string JwtSecret = "test-secret-please-change-in-tests-only-1234567890";
    private readonly TestWebApplicationFactory _factory;

    public TenantOnboardingTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Tenant_Onboarding_HappyPath_CompletesVerificationAndPasswordChange()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var otpStore = scope.ServiceProvider.GetRequiredService<TestOtpStore>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var organization = new Organization { Id = 1, Name = "Org", Status = OrganizationStatus.Active };
        var tenant = new Tenant
        {
            Id = 100,
            FirstName = "Tenant",
            LastName = "Invite",
            Email = "tenant-invite@hisa.local",
            PhoneNumber = "+254733300001",
            Status = TenantStatus.Active
        };
        var user = new User
        {
            Id = 101,
            FirstName = "Tenant",
            LastName = "Invite",
            Email = tenant.Email,
            PhoneNumber = tenant.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("HisaRentalsTenant@2025"),
            Role = UserRole.Tenant,
            Status = UserStatus.Invited,
            IsVerified = false,
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

        var sendResponse = await client.PostAsJsonAsync("/api/auth/verification/send", new { channel = "Email" });
        sendResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var code = otpStore.GetEmailCode(user.Email);
        code.Should().NotBeNull();

        var verifyResponse = await client.PostAsJsonAsync("/api/auth/verification/verify", new { code });
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var passwordChange = await client.PostAsJsonAsync("/api/auth/complete-password-change", new
        {
            newPassword = "HisaRentalsTenant@2026",
            confirmPassword = "HisaRentalsTenant@2026"
        });
        passwordChange.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await context.Users.FindAsync(user.Id);
        updated.Should().NotBeNull();
        updated!.IsVerified.Should().BeTrue();
        updated.MustChangePassword.Should().BeFalse();
        updated.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public async Task VerificationOtp_Resend_IsRateLimited()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var organization = new Organization { Id = 1, Name = "Org", Status = OrganizationStatus.Active };
        var user = new User
        {
            Id = 120,
            FirstName = "Resend",
            LastName = "User",
            Email = "resend@hisa.local",
            PhoneNumber = "+254733300120",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("HisaRentals@2025"),
            Role = UserRole.Manager,
            Status = UserStatus.Active,
            IsVerified = false,
            OrganizationId = organization.Id
        };

        context.Organizations.Add(organization);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(user));

        var first = await client.PostAsJsonAsync("/api/auth/verification/send", new { channel = "Email" });
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var second = await client.PostAsJsonAsync("/api/auth/verification/send", new { channel = "Email" });
        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
