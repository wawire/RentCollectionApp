using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RentCollection.Application.DTOs.Auth;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;
using RentCollection.Infrastructure.Repositories.Implementations;
using RentCollection.Infrastructure.Services.Auth;
using Xunit;

namespace RentCollection.UnitTests.Auth;

public class CompletePasswordChangeTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static IConfiguration CreateConfig()
    {
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = "test-secret-please-change-in-tests-only-1234567890",
            ["Jwt:Issuer"] = "HisaRentalsAPI",
            ["Jwt:Audience"] = "HisaRentals"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    [Fact]
    public async Task CompletePasswordChangeAsync_ActivatesInvitedVerifiedUser()
    {
        using var context = CreateDbContext();
        var user = new User
        {
            FirstName = "Tenant",
            LastName = "Invite",
            Email = "tenant@hisa.local",
            PhoneNumber = "+254700000111",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("HisaRentalsTenant@2025"),
            Role = UserRole.Tenant,
            Status = UserStatus.Invited,
            IsVerified = true,
            MustChangePassword = true,
            OrganizationId = 1
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var userRepository = new UserRepository(context);
        var authService = new AuthService(
            userRepository,
            Mock.Of<IPropertyRepository>(),
            Mock.Of<IAuditLogService>(),
            Mock.Of<IEmailService>(),
            Mock.Of<IVerificationService>(),
            context,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<AutoMapper.IMapper>(),
            Mock.Of<ILogger<AuthService>>(),
            CreateConfig(),
            Mock.Of<ICurrentUserService>());

        var dto = new CompletePasswordChangeDto
        {
            NewPassword = "HisaRentalsTenant@2026",
            ConfirmPassword = "HisaRentalsTenant@2026"
        };

        var response = await authService.CompletePasswordChangeAsync(user.Id, dto);

        response.MustChangePassword.Should().BeFalse();

        var updated = await context.Users.FindAsync(user.Id);
        updated.Should().NotBeNull();
        updated!.MustChangePassword.Should().BeFalse();
        updated.Status.Should().Be(UserStatus.Active);
    }
}
