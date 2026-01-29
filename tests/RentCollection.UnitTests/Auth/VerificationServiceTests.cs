using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RentCollection.Application.Common.Exceptions;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;
using RentCollection.Infrastructure.Repositories.Implementations;
using RentCollection.Infrastructure.Services;
using Xunit;

namespace RentCollection.UnitTests.Auth;

public class VerificationServiceTests
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
            ["Verification:OtpExpiryMinutes"] = "5",
            ["Verification:OtpMaxAttempts"] = "2",
            ["Verification:OtpResendCooldownSeconds"] = "60",
            ["Verification:OtpLockoutMinutes"] = "10",
            ["Verification:OtpSecret"] = "test-otp-secret"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    [Fact]
    public async Task SendVerificationOtpAsync_EnforcesCooldown()
    {
        using var context = CreateDbContext();
        var user = new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@hisa.local",
            PhoneNumber = "+254700000000",
            PasswordHash = "hash",
            Role = UserRole.Manager,
            Status = UserStatus.Active,
            OrganizationId = 1
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var userRepository = new UserRepository(context);
        var emailService = new Mock<IEmailService>();
        var smsService = new Mock<ISmsService>();
        var auditLogService = new Mock<IAuditLogService>();

        var service = new VerificationService(
            userRepository,
            emailService.Object,
            smsService.Object,
            auditLogService.Object,
            CreateConfig(),
            Mock.Of<ILogger<VerificationService>>());

        await service.SendVerificationOtpAsync(user.Id, VerificationChannel.Email);

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            service.SendVerificationOtpAsync(user.Id, VerificationChannel.Email));

        exception.Message.Should().Contain("Please wait before requesting another code");
    }

    [Fact]
    public async Task VerifyOtpAsync_ActivatesInvitedUser_WhenPasswordChangeNotRequired()
    {
        using var context = CreateDbContext();
        var user = new User
        {
            FirstName = "Invite",
            LastName = "User",
            Email = "invite@hisa.local",
            PhoneNumber = "+254711111111",
            PasswordHash = "hash",
            Role = UserRole.Manager,
            Status = UserStatus.Invited,
            OrganizationId = 1,
            MustChangePassword = false
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var userRepository = new UserRepository(context);
        var emailService = new Mock<IEmailService>();
        var otpCode = "";
        emailService.Setup(s => s.SendVerificationOtpEmailAsync(user.Email, user.FullName, It.IsAny<string>()))
            .Callback<string, string, string>((_, _, code) => otpCode = code)
            .Returns(Task.CompletedTask);

        var service = new VerificationService(
            userRepository,
            emailService.Object,
            Mock.Of<ISmsService>(),
            Mock.Of<IAuditLogService>(),
            CreateConfig(),
            Mock.Of<ILogger<VerificationService>>());

        await service.SendVerificationOtpAsync(user.Id, VerificationChannel.Email);
        await service.VerifyOtpAsync(user.Id, otpCode);

        var updated = await context.Users.FindAsync(user.Id);
        updated.Should().NotBeNull();
        updated!.IsVerified.Should().BeTrue();
        updated.Status.Should().Be(UserStatus.Active);
        updated.OtpHash.Should().BeNull();
    }

    [Fact]
    public async Task VerifyOtpAsync_LocksOutAfterMaxAttempts()
    {
        using var context = CreateDbContext();
        var user = new User
        {
            FirstName = "Lock",
            LastName = "User",
            Email = "lock@hisa.local",
            PhoneNumber = "+254722222222",
            PasswordHash = "hash",
            Role = UserRole.Manager,
            Status = UserStatus.Active,
            OrganizationId = 1
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var userRepository = new UserRepository(context);
        var emailService = new Mock<IEmailService>();
        var otpCode = "";
        emailService.Setup(s => s.SendVerificationOtpEmailAsync(user.Email, user.FullName, It.IsAny<string>()))
            .Callback<string, string, string>((_, _, code) => otpCode = code)
            .Returns(Task.CompletedTask);

        var service = new VerificationService(
            userRepository,
            emailService.Object,
            Mock.Of<ISmsService>(),
            Mock.Of<IAuditLogService>(),
            CreateConfig(),
            Mock.Of<ILogger<VerificationService>>());

        await service.SendVerificationOtpAsync(user.Id, VerificationChannel.Email);

        await Assert.ThrowsAsync<BadRequestException>(() => service.VerifyOtpAsync(user.Id, "000000"));
        await Assert.ThrowsAsync<BadRequestException>(() => service.VerifyOtpAsync(user.Id, "111111"));

        var locked = await context.Users.FindAsync(user.Id);
        locked.Should().NotBeNull();
        locked!.OtpLockoutUntil.Should().NotBeNull();

        var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.VerifyOtpAsync(user.Id, otpCode));
        exception.Message.Should().Contain("Too many attempts");
    }

    [Fact]
    public async Task VerifyOtpAsync_ExpiredCodeThrows()
    {
        using var context = CreateDbContext();
        var user = new User
        {
            FirstName = "Expired",
            LastName = "User",
            Email = "expired@hisa.local",
            PhoneNumber = "+254733333333",
            PasswordHash = "hash",
            Role = UserRole.Manager,
            Status = UserStatus.Active,
            OrganizationId = 1
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var userRepository = new UserRepository(context);
        var emailService = new Mock<IEmailService>();
        var otpCode = "";
        emailService.Setup(s => s.SendVerificationOtpEmailAsync(user.Email, user.FullName, It.IsAny<string>()))
            .Callback<string, string, string>((_, _, code) => otpCode = code)
            .Returns(Task.CompletedTask);

        var service = new VerificationService(
            userRepository,
            emailService.Object,
            Mock.Of<ISmsService>(),
            Mock.Of<IAuditLogService>(),
            CreateConfig(),
            Mock.Of<ILogger<VerificationService>>());

        await service.SendVerificationOtpAsync(user.Id, VerificationChannel.Email);

        user.OtpExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        context.Users.Update(user);
        await context.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.VerifyOtpAsync(user.Id, otpCode));
        exception.Message.Should().Contain("expired");
    }
}
