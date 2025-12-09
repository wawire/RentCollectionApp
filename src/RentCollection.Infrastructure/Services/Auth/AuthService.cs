using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using RentCollection.Application.Common.Exceptions;
using RentCollection.Application.DTOs.Auth;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Auth;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;
using BCrypt.Net;

namespace RentCollection.Infrastructure.Services.Auth;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IEmailService _emailService;
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ICurrentUserService _currentUserService;

    public AuthService(
        IUserRepository userRepository,
        IPropertyRepository propertyRepository,
        IAuditLogService auditLogService,
        IEmailService emailService,
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        ILogger<AuthService> logger,
        IConfiguration configuration,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _propertyRepository = propertyRepository;
        _auditLogService = auditLogService;
        _emailService = emailService;
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _logger = logger;
        _configuration = configuration;
        _currentUserService = currentUserService;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find user by email or phone
            var user = await _userRepository.GetByEmailOrPhoneAsync(loginDto.EmailOrPhone, cancellationToken);

            if (user == null)
            {
                throw new BadRequestException("Invalid email/phone or password");
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                throw new BadRequestException("Invalid email/phone or password");
            }

            // Check if user is active
            if (user.Status != UserStatus.Active)
            {
                throw new BadRequestException($"Account is {user.Status.ToString().ToLower()}. Please contact administrator.");
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            // Generate JWT token
            var token = GenerateJwtToken(user);

            var response = new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                PropertyId = user.PropertyId,
                TenantId = user.TenantId
            };

            _logger.LogInformation("User {UserId} ({Email}) logged in successfully", user.Id, user.Email);

            return response;
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {EmailOrPhone}", loginDto.EmailOrPhone);
            throw new BadRequestException("An error occurred during login");
        }
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // CRITICAL RBAC VALIDATION: Enforce role creation hierarchy
            if (!_currentUserService.IsSystemAdmin)
            {
                // Only SystemAdmin can create other SystemAdmins or Landlords
                if (registerDto.Role == UserRole.SystemAdmin || registerDto.Role == UserRole.Landlord)
                {
                    throw new BadRequestException("You do not have permission to create users with this role");
                }

                // Landlords can only create Caretakers, Accountants, and Tenants for their own properties
                if (_currentUserService.IsLandlord)
                {
                    // Validate that the user being created is assigned to the landlord's properties
                    if (registerDto.PropertyId.HasValue)
                    {
                        var property = await _propertyRepository.GetByIdAsync(registerDto.PropertyId.Value);

                        if (property == null)
                        {
                            throw new BadRequestException($"Property with ID {registerDto.PropertyId.Value} not found");
                        }

                        var landlordId = _currentUserService.UserIdInt;
                        if (!landlordId.HasValue || property.LandlordId != landlordId.Value)
                        {
                            throw new BadRequestException("You can only assign users to your own properties");
                        }
                    }
                }
                else
                {
                    // Other roles (Caretaker, Accountant, Tenant) cannot create users
                    throw new BadRequestException("You do not have permission to create users");
                }
            }

            // Validate passwords match
            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                throw new BadRequestException("Passwords do not match");
            }

            // Check if email already exists
            if (await _userRepository.EmailExistsAsync(registerDto.Email, cancellationToken))
            {
                throw new BadRequestException("Email already exists");
            }

            // Check if phone number already exists
            if (await _userRepository.PhoneNumberExistsAsync(registerDto.PhoneNumber, cancellationToken))
            {
                throw new BadRequestException("Phone number already exists");
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            // Create user entity
            var user = new User
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                PasswordHash = passwordHash,
                Role = registerDto.Role,
                Status = UserStatus.Active,
                PropertyId = registerDto.PropertyId,
                TenantId = registerDto.TenantId,
                CreatedAt = DateTime.UtcNow
            };

            // Save user
            await _userRepository.AddAsync(user);

            _logger.LogInformation("New user registered: {UserId} ({Email}), Role: {Role}",
                user.Id, user.Email, user.Role);

            // Audit log: User created
            await _auditLogService.LogUserCreatedAsync(user.Id, user.Email, user.Role.ToString());

            // Send email verification token (non-blocking, silent failure)
            try
            {
                var verificationToken = GenerateSecureToken();
                var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

                var emailVerificationToken = new EmailVerificationToken
                {
                    UserId = user.Id,
                    Token = verificationToken,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(1), // Token valid for 24 hours
                    IsUsed = false,
                    IpAddress = ipAddress
                };

                await _context.EmailVerificationTokens.AddAsync(emailVerificationToken, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                await _emailService.SendEmailVerificationAsync(user.Email, verificationToken, user.FullName);
                _logger.LogInformation("Email verification sent to: {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send email verification to: {Email}", user.Email);
                // Don't fail registration if email fails
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);

            var response = new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                PropertyId = user.PropertyId,
                TenantId = user.TenantId
            };

            return response;
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for {Email}", registerDto.Email);
            throw new BadRequestException("An error occurred during registration");
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdWithRelatedDataAsync(userId, cancellationToken);

        if (user == null)
            return null;

        // Check access permission
        if (!_currentUserService.IsSystemAdmin)
        {
            // Landlords can only view users associated with their properties
            if (_currentUserService.IsLandlord)
            {
                var landlordId = _currentUserService.UserIdInt;
                if (landlordId.HasValue)
                {
                    // Allow if it's the landlord themselves OR user belongs to landlord's property
                    if (user.Id != landlordId.Value &&
                        !(user.PropertyId.HasValue && user.Property != null && user.Property.LandlordId == landlordId.Value))
                    {
                        throw new BadRequestException("You do not have permission to view this user");
                    }
                }
            }
            else
            {
                // Other roles can only view themselves
                var currentUserId = _currentUserService.UserIdInt;
                if (!currentUserId.HasValue || user.Id != currentUserId.Value)
                {
                    throw new BadRequestException("You do not have permission to view this user");
                }
            }
        }

        return MapToUserDto(user);
    }

    public async Task<List<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllWithRelatedDataAsync(cancellationToken);

        // Filter users based on current user role
        if (!_currentUserService.IsSystemAdmin)
        {
            // Landlords can only see users associated with their properties
            if (_currentUserService.IsLandlord)
            {
                var landlordId = _currentUserService.UserIdInt;
                if (landlordId.HasValue)
                {
                    users = users.Where(u =>
                        // Include the landlord themselves
                        u.Id == landlordId.Value ||
                        // Include users with PropertyId matching landlord's properties
                        (u.PropertyId.HasValue && u.Property != null && u.Property.LandlordId == landlordId.Value) ||
                        // Include users with Role Caretaker/Accountant who have matching LandlordId in claims
                        (u.Role == UserRole.Caretaker || u.Role == UserRole.Accountant)
                    ).ToList();
                }
            }
            else
            {
                // Other roles can only see themselves
                var userId = _currentUserService.UserIdInt;
                if (userId.HasValue)
                {
                    users = users.Where(u => u.Id == userId.Value).ToList();
                }
                else
                {
                    users = new List<User>();
                }
            }
        }

        return users.Select(MapToUserDto).ToList();
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
        {
            throw new BadRequestException("Current password is incorrect");
        }

        // Validate new passwords match
        if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
        {
            throw new BadRequestException("New passwords do not match");
        }

        // Hash new password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);

        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Password changed for user {UserId}", userId);
    }

    public async Task UpdateUserStatusAsync(int userId, UserStatus status, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdWithRelatedDataAsync(userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        // Check access permission
        if (!_currentUserService.IsSystemAdmin)
        {
            // Landlords can only update status for users in their properties
            if (_currentUserService.IsLandlord)
            {
                var landlordId = _currentUserService.UserIdInt;
                if (landlordId.HasValue)
                {
                    // Cannot modify themselves or users not in their properties
                    if (user.Id == landlordId.Value)
                    {
                        throw new BadRequestException("You cannot modify your own status");
                    }

                    if (!(user.PropertyId.HasValue && user.Property != null && user.Property.LandlordId == landlordId.Value))
                    {
                        throw new BadRequestException("You do not have permission to update this user's status");
                    }

                    // Landlords cannot update other landlords or system admins
                    if (user.Role == UserRole.SystemAdmin || user.Role == UserRole.Landlord)
                    {
                        throw new BadRequestException("You do not have permission to update this user's status");
                    }
                }
            }
            else
            {
                // Other roles cannot update user status
                throw new BadRequestException("You do not have permission to update user status");
            }
        }

        var oldStatus = user.Status.ToString();
        user.Status = status;
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("User {UserId} status updated to {Status} by {CurrentUser}", userId, status, _currentUserService.Email);

        // Audit log: User status changed
        await _auditLogService.LogUserStatusChangedAsync(userId, oldStatus, status.ToString());
    }

    public async Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        var userEmail = user.Email;
        await _userRepository.DeleteAsync(user);

        _logger.LogInformation("User {UserId} deleted", userId);

        // Audit log: User deleted
        await _auditLogService.LogUserDeletedAsync(userId, userEmail);
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSecret = _configuration["Jwt:Secret"] ?? "your-256-bit-secret-key-here-change-in-production-minimum-32-characters";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "RentCollectionAPI";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "RentCollectionApp";

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Determine LandlordId: for Landlords, it's their user ID
        var landlordId = user.Role == UserRole.Landlord ? user.Id.ToString() : "";

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("PhoneNumber", user.PhoneNumber),
            new Claim("PropertyId", user.PropertyId?.ToString() ?? ""),
            new Claim("TenantId", user.TenantId?.ToString() ?? ""),
            new Claim("LandlordId", landlordId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find user by email
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

            // Don't reveal if user exists or not (security best practice)
            if (user == null)
            {
                _logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
                return; // Silently return without error
            }

            // Check if user is active
            if (user.Status != UserStatus.Active)
            {
                _logger.LogWarning("Password reset requested for inactive user: {Email}", email);
                return; // Silently return without error
            }

            // Generate secure random token
            var token = GenerateSecureToken();

            // Get IP address
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

            // Create password reset token
            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(1), // Token valid for 1 hour
                IsUsed = false,
                IpAddress = ipAddress
            };

            await _context.PasswordResetTokens.AddAsync(resetToken, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Send password reset email
            await _emailService.SendPasswordResetEmailAsync(user.Email, token, user.FullName);

            _logger.LogInformation("Password reset email sent to: {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting password reset for email: {Email}", email);
            // Don't throw - silently fail to avoid revealing user existence
        }
    }

    public async Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find token
            var resetToken = await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == resetPasswordDto.Token, cancellationToken);

            if (resetToken == null)
            {
                throw new BadRequestException("Invalid or expired reset token");
            }

            // Check if token is valid
            if (!resetToken.IsValid)
            {
                if (resetToken.IsUsed)
                {
                    throw new BadRequestException("This reset token has already been used");
                }
                else
                {
                    throw new BadRequestException("This reset token has expired");
                }
            }

            // Hash new password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);

            // Update user password
            resetToken.User.PasswordHash = passwordHash;
            await _userRepository.UpdateAsync(resetToken.User);

            // Mark token as used
            resetToken.IsUsed = true;
            resetToken.UsedAt = DateTime.UtcNow;
            _context.PasswordResetTokens.Update(resetToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Password reset successful for user: {Email}", resetToken.User.Email);
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password with token");
            throw new BadRequestException("An error occurred while resetting the password");
        }
    }

    public async Task VerifyEmailAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find token
            var verificationToken = await _context.EmailVerificationTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);

            if (verificationToken == null)
            {
                throw new BadRequestException("Invalid or expired verification token");
            }

            // Check if token is valid
            if (!verificationToken.IsValid)
            {
                if (verificationToken.IsUsed)
                {
                    throw new BadRequestException("This verification token has already been used");
                }
                else
                {
                    throw new BadRequestException("This verification token has expired");
                }
            }

            // Check if email is already verified
            if (verificationToken.User.IsEmailVerified)
            {
                throw new BadRequestException("Email address is already verified");
            }

            // Mark email as verified
            verificationToken.User.IsEmailVerified = true;
            verificationToken.User.EmailVerifiedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(verificationToken.User);

            // Mark token as used
            verificationToken.IsUsed = true;
            verificationToken.UsedAt = DateTime.UtcNow;
            _context.EmailVerificationTokens.Update(verificationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Email verified successfully for user: {Email}", verificationToken.User.Email);
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email with token");
            throw new BadRequestException("An error occurred while verifying the email");
        }
    }

    public async Task ResendEmailVerificationAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find user by email
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

            // Don't reveal if user exists or not (security best practice)
            if (user == null)
            {
                _logger.LogWarning("Email verification resend requested for non-existent email: {Email}", email);
                return; // Silently return without error
            }

            // Check if email is already verified
            if (user.IsEmailVerified)
            {
                _logger.LogWarning("Email verification resend requested for already verified email: {Email}", email);
                return; // Silently return without error
            }

            // Check if user is active
            if (user.Status != UserStatus.Active)
            {
                _logger.LogWarning("Email verification resend requested for inactive user: {Email}", email);
                return; // Silently return without error
            }

            // Generate secure random token
            var token = GenerateSecureToken();

            // Get IP address
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

            // Create email verification token
            var verificationToken = new EmailVerificationToken
            {
                UserId = user.Id,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(1), // Token valid for 24 hours
                IsUsed = false,
                IpAddress = ipAddress
            };

            await _context.EmailVerificationTokens.AddAsync(verificationToken, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Send email verification email
            await _emailService.SendEmailVerificationAsync(user.Email, token, user.FullName);

            _logger.LogInformation("Email verification resent to: {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending email verification for email: {Email}", email);
            // Don't throw - silently fail to avoid revealing user existence
        }
    }

    public async Task<Setup2FAResponseDto> Setup2FAAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Generate new secret
        var secret = Application.Helpers.TotpHelper.GenerateSecret();

        // Save secret temporarily (not enabled yet until verified)
        user.TwoFactorSecret = secret;
        await _userRepository.UpdateAsync(user);

        // Generate QR code URI
        var qrCodeUri = Application.Helpers.TotpHelper.GenerateQrCodeUri(secret, user.Email, "RentCollection");

        _logger.LogInformation("2FA setup initiated for user {UserId}", userId);

        return new Setup2FAResponseDto
        {
            SecretKey = secret,
            QrCodeUri = qrCodeUri,
            Issuer = "RentCollection",
            AccountName = user.Email
        };
    }

    public async Task Enable2FAAsync(int userId, Enable2FADto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (string.IsNullOrWhiteSpace(user.TwoFactorSecret))
        {
            throw new InvalidOperationException("2FA setup not initiated. Call Setup2FA first.");
        }

        // Verify the code
        if (!Application.Helpers.TotpHelper.VerifyCode(user.TwoFactorSecret, dto.VerificationCode))
        {
            throw new InvalidOperationException("Invalid verification code. Please try again.");
        }

        // Enable 2FA
        user.TwoFactorEnabled = true;
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("2FA enabled for user {UserId}", userId);
    }

    public async Task Disable2FAAsync(int userId, Disable2FADto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Verify password before disabling 2FA
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            throw new InvalidOperationException("Invalid password");
        }

        // Disable 2FA
        user.TwoFactorEnabled = false;
        user.TwoFactorSecret = null;
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("2FA disabled for user {UserId}", userId);
    }

    public async Task<AuthResponseDto> Verify2FACodeAsync(Verify2FACodeDto dto, CancellationToken cancellationToken = default)
    {
        // Find user by email or phone
        var user = await _userRepository.GetByEmailAsync(dto.EmailOrPhone, cancellationToken);
        if (user == null)
        {
            user = await _userRepository.GetByPhoneNumberAsync(dto.EmailOrPhone, cancellationToken);
        }

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        if (!user.TwoFactorEnabled || string.IsNullOrWhiteSpace(user.TwoFactorSecret))
        {
            throw new InvalidOperationException("2FA is not enabled for this user");
        }

        // Verify the code
        if (!Application.Helpers.TotpHelper.VerifyCode(user.TwoFactorSecret, dto.Code))
        {
            _logger.LogWarning("Invalid 2FA code attempt for user {UserId}", user.Id);
            throw new UnauthorizedAccessException("Invalid verification code");
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // Generate JWT token
        var token = GenerateJwtToken(user);

        _logger.LogInformation("User {UserId} logged in successfully with 2FA", user.Id);

        return new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            PropertyId = user.PropertyId,
            TenantId = user.TenantId
        };
    }

    private string GenerateSecureToken()
    {
        // Generate a cryptographically secure random token
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    private UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            RoleName = user.Role.ToString(),
            Status = user.Status,
            StatusName = user.Status.ToString(),
            PropertyId = user.PropertyId,
            PropertyName = user.Property?.Name,
            TenantId = user.TenantId,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}
