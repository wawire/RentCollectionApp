using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Auth;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Infrastructure.Repositories.Interfaces;
using RentCollection.Infrastructure.Services;
using BCrypt.Net;

namespace RentCollection.Application.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IMapper mapper,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _mapper = mapper;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto loginDto)
    {
        try
        {
            var user = await _userRepository.GetByUsernameAsync(loginDto.Username);

            if (user == null)
            {
                _logger.LogWarning("Login attempt failed: User '{Username}' not found", loginDto.Username);
                return Result<AuthResponseDto>.Failure("Invalid username or password");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt failed: User '{Username}' is inactive", loginDto.Username);
                return Result<AuthResponseDto>.Failure("Your account has been deactivated. Please contact support.");
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login attempt failed: Invalid password for user '{Username}'", loginDto.Username);
                return Result<AuthResponseDto>.Failure("Invalid username or password");
            }

            // Generate tokens
            var accessToken = _jwtTokenService.GenerateAccessToken(user);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            // Update user with refresh token
            var refreshTokenExpiryDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "7");
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshTokenExpiryDays);
            user.LastLoginAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            var response = new AuthResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:AccessTokenExpiryMinutes"] ?? "60"))
            };

            _logger.LogInformation("User '{Username}' logged in successfully", user.Username);
            return Result<AuthResponseDto>.Success(response, "Login successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user '{Username}'", loginDto.Username);
            return Result<AuthResponseDto>.Failure("An error occurred during login");
        }
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            // Validate username doesn't exist
            if (await _userRepository.UsernameExistsAsync(registerDto.Username))
            {
                return Result<AuthResponseDto>.Failure("Username is already taken");
            }

            // Validate email doesn't exist
            if (await _userRepository.EmailExistsAsync(registerDto.Email))
            {
                return Result<AuthResponseDto>.Failure("Email is already registered");
            }

            // Validate role
            var validRoles = new[] { "Admin", "PropertyManager", "Viewer" };
            if (!validRoles.Contains(registerDto.Role))
            {
                return Result<AuthResponseDto>.Failure("Invalid role specified");
            }

            // Create new user
            var user = new ApplicationUser
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                Role = registerDto.Role,
                IsActive = true
            };

            await _userRepository.AddAsync(user);

            // Generate tokens
            var accessToken = _jwtTokenService.GenerateAccessToken(user);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            // Update user with refresh token
            var refreshTokenExpiryDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "7");
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshTokenExpiryDays);
            user.LastLoginAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            var response = new AuthResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:AccessTokenExpiryMinutes"] ?? "60"))
            };

            _logger.LogInformation("User '{Username}' registered successfully", user.Username);
            return Result<AuthResponseDto>.Success(response, "Registration successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user '{Username}'", registerDto.Username);
            return Result<AuthResponseDto>.Failure("An error occurred during registration");
        }
    }

    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);

            if (user == null)
            {
                return Result<AuthResponseDto>.Failure("Invalid or expired refresh token");
            }

            // Generate new tokens
            var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

            // Update user with new refresh token
            var refreshTokenExpiryDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "7");
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshTokenExpiryDays);

            await _userRepository.UpdateAsync(user);

            var response = new AuthResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:AccessTokenExpiryMinutes"] ?? "60"))
            };

            _logger.LogInformation("Refresh token used for user '{Username}'", user.Username);
            return Result<AuthResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return Result<AuthResponseDto>.Failure("An error occurred during token refresh");
        }
    }

    public async Task<Result> LogoutAsync(int userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return Result.Failure("User not found");
            }

            // Clear refresh token
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("User '{Username}' logged out successfully", user.Username);
            return Result.Success("Logout successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user ID: {UserId}", userId);
            return Result.Failure("An error occurred during logout");
        }
    }

    public async Task<Result<UserDto>> GetUserByIdAsync(int userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return Result<UserDto>.Failure("User not found");
            }

            var userDto = _mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID: {UserId}", userId);
            return Result<UserDto>.Failure("An error occurred while retrieving user");
        }
    }

    public async Task<Result<IEnumerable<UserDto>>> GetAllUsersAsync()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);

            return Result<IEnumerable<UserDto>>.Success(userDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            return Result<IEnumerable<UserDto>>.Failure("An error occurred while retrieving users");
        }
    }
}
