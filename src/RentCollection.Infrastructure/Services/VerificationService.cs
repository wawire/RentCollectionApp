using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Common.Exceptions;
using RentCollection.Application.DTOs.Sms;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Enums;

namespace RentCollection.Infrastructure.Services;

public class VerificationService : IVerificationService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly IAuditLogService _auditLogService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<VerificationService> _logger;

    public VerificationService(
        IUserRepository userRepository,
        IEmailService emailService,
        ISmsService smsService,
        IAuditLogService auditLogService,
        IConfiguration configuration,
        ILogger<VerificationService> logger)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _smsService = smsService;
        _auditLogService = auditLogService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendVerificationOtpAsync(int userId, VerificationChannel channel, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new BadRequestException("User not found");
        }

        if (user.Status == UserStatus.Suspended || user.Status == UserStatus.Inactive)
        {
            throw new BadRequestException("Account is not active");
        }

        var now = DateTime.UtcNow;
        var cooldownSeconds = GetConfigInt("Verification:OtpResendCooldownSeconds", 60);
        if (user.OtpLastSentAt.HasValue && (now - user.OtpLastSentAt.Value).TotalSeconds < cooldownSeconds)
        {
            throw new BadRequestException("Please wait before requesting another code.");
        }

        if (user.OtpLockoutUntil.HasValue && user.OtpLockoutUntil.Value > now)
        {
            throw new BadRequestException("Account is temporarily locked. Please try again later.");
        }

        var code = GenerateOtpCode();
        var expiryMinutes = GetConfigInt("Verification:OtpExpiryMinutes", 10);

        user.OtpHash = HashOtp(code);
        user.OtpExpiresAt = now.AddMinutes(expiryMinutes);
        user.OtpAttempts = 0;
        user.OtpLastSentAt = now;
        user.OtpLockoutUntil = null;
        user.VerificationChannel = channel;

        await _userRepository.UpdateAsync(user);

        if (channel == VerificationChannel.Email)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new BadRequestException("Email address is required for verification");
            }

            await _emailService.SendVerificationOtpEmailAsync(user.Email, user.FullName, code);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                throw new BadRequestException("Phone number is required for verification");
            }

            var message = $"Your Hisa Rentals verification code is {code}. It expires in {expiryMinutes} minutes.";
            await _smsService.SendSmsAsync(new SendSmsDto
            {
                PhoneNumber = user.PhoneNumber,
                Message = message,
                TenantId = user.TenantId
            });
        }

        _logger.LogInformation("Verification OTP sent to user {UserId} via {Channel}", user.Id, channel);
    }

    public async Task VerifyOtpAsync(int userId, string code, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new BadRequestException("User not found");
        }

        if (user.IsVerified)
        {
            return;
        }

        var now = DateTime.UtcNow;
        if (user.OtpLockoutUntil.HasValue && user.OtpLockoutUntil.Value > now)
        {
            throw new BadRequestException("Too many attempts. Please try again later.");
        }

        if (!user.OtpExpiresAt.HasValue || user.OtpExpiresAt.Value < now || string.IsNullOrWhiteSpace(user.OtpHash))
        {
            throw new BadRequestException("Verification code has expired. Please request a new one.");
        }

        if (!VerifyOtp(code, user.OtpHash))
        {
            user.OtpAttempts += 1;
            var maxAttempts = GetConfigInt("Verification:OtpMaxAttempts", 5);
            if (user.OtpAttempts >= maxAttempts)
            {
                var lockoutMinutes = GetConfigInt("Verification:OtpLockoutMinutes", 15);
                user.OtpLockoutUntil = now.AddMinutes(lockoutMinutes);
            }

            await _userRepository.UpdateAsync(user);
            throw new BadRequestException("Invalid verification code.");
        }

        user.IsVerified = true;
        user.VerifiedAt = now;
        user.OtpHash = null;
        user.OtpExpiresAt = null;
        user.OtpAttempts = 0;
        user.OtpLockoutUntil = null;

        if (user.Status == UserStatus.Invited && !user.MustChangePassword)
        {
            user.Status = UserStatus.Active;
        }

        await _userRepository.UpdateAsync(user);

        await _auditLogService.LogActionAsync("User.Verified", "User", user.Id, $"Verified via {user.VerificationChannel}");
        _logger.LogInformation("User {UserId} verified successfully", user.Id);
    }

    private string GenerateOtpCode()
    {
        var rng = RandomNumberGenerator.GetInt32(100000, 999999);
        return rng.ToString();
    }

    private string HashOtp(string code)
    {
        var secret = _configuration["Verification:OtpSecret"] ?? _configuration["Jwt:Secret"] ?? "hisa-otp-secret";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(code));
        return Convert.ToBase64String(hash);
    }

    private bool VerifyOtp(string code, string hash)
    {
        var computed = HashOtp(code);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(computed),
            Convert.FromBase64String(hash));
    }

    private int GetConfigInt(string key, int fallback)
    {
        var raw = _configuration[key];
        return int.TryParse(raw, out var value) ? value : fallback;
    }
}
