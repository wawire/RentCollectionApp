using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using RentCollection.Application.Common.Exceptions;
using RentCollection.Application.DTOs.Auth;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Auth;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using BCrypt.Net;

namespace RentCollection.Infrastructure.Services.Auth;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<AuthService> logger,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
        _configuration = configuration;
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

        return MapToUserDto(user);
    }

    public async Task<List<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllWithRelatedDataAsync(cancellationToken);
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
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        user.Status = status;
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("User {UserId} status updated to {Status}", userId, status);
    }

    public async Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        await _userRepository.DeleteAsync(user);

        _logger.LogInformation("User {UserId} deleted", userId);
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
