using RentCollection.Application.DTOs.Auth;

namespace RentCollection.Application.Services.Auth;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticate user with email/phone and password
    /// </summary>
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Register a new user
    /// </summary>
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<UserDto?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all users (for admin)
    /// </summary>
    Task<List<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Change user password
    /// </summary>
    Task ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update user status (activate/suspend)
    /// </summary>
    Task UpdateUserStatusAsync(int userId, Domain.Enums.UserStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete user
    /// </summary>
    Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default);
}
