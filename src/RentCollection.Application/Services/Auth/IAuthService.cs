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
    /// Complete forced password change for invited users
    /// </summary>
    Task<AuthResponseDto> CompletePasswordChangeAsync(int userId, CompletePasswordChangeDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update user status (activate/suspend)
    /// </summary>
    Task UpdateUserStatusAsync(int userId, Domain.Enums.UserStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete user
    /// </summary>
    Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Request password reset (sends email with reset token)
    /// </summary>
    Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset password using a valid token
    /// </summary>
    Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify email address using a valid token
    /// </summary>
    Task VerifyEmailAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resend email verification token
    /// </summary>
    Task ResendEmailVerificationAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Setup two-factor authentication for a user
    /// Generates a new secret and QR code
    /// </summary>
    Task<Setup2FAResponseDto> Setup2FAAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enable two-factor authentication after verifying the setup
    /// </summary>
    Task Enable2FAAsync(int userId, Enable2FADto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disable two-factor authentication
    /// </summary>
    Task Disable2FAAsync(int userId, Disable2FADto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify 2FA code during login
    /// </summary>
    Task<AuthResponseDto> Verify2FACodeAsync(Verify2FACodeDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresh authentication token and user payload
    /// </summary>
    Task<AuthResponseDto> RefreshAuthAsync(int userId, CancellationToken cancellationToken = default);
}
