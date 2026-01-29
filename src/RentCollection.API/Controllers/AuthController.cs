using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.Authorization;
using RentCollection.Application.DTOs.Auth;
using RentCollection.Application.Services.Auth;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Enums;
using System.Security.Claims;

namespace RentCollection.API.Controllers;

/// <summary>
/// Authentication and user management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IVerificationService _verificationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, IVerificationService verificationService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _verificationService = verificationService;
        _logger = logger;
    }

    /// <summary>
    /// Login with email/phone and password
    /// </summary>
    /// <param name="loginDto">Login credentials</param>
    /// <returns>Authentication token and user info</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var response = await _authService.LoginAsync(loginDto);
        return Ok(response);
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="registerDto">User registration details</param>
    /// <returns>Authentication token and user info</returns>
    [HttpPost("register")]
    [Authorize(Roles = "PlatformAdmin,Landlord")] // Only PlatformAdmin and Landlord can create users
    [Authorize(Policy = Policies.RequireVerifiedUser)]
    [Authorize(Policy = Policies.RequireActiveOrganization)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var response = await _authService.RegisterAsync(registerDto);
        return CreatedAtAction(nameof(GetUser), new { id = response.UserId }, response);
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    /// <returns>Current user details</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        if (userId == 0)
            return Unauthorized();

        var user = await _authService.GetUserByIdAsync(userId);

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("{id}")]
    [Authorize(Roles = "PlatformAdmin,Landlord")]
    [Authorize(Policy = Policies.RequireVerifiedUser)]
    [Authorize(Policy = Policies.RequireActiveOrganization)]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _authService.GetUserByIdAsync(id);

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    /// <summary>
    /// Get all users
    /// </summary>
    /// <returns>List of all users</returns>
    [HttpGet]
    [Authorize(Roles = "PlatformAdmin,Landlord")]
    [Authorize(Policy = Policies.RequireVerifiedUser)]
    [Authorize(Policy = Policies.RequireActiveOrganization)]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _authService.GetAllUsersAsync();
        return Ok(users);
    }

    /// <summary>
    /// Change password for current user
    /// </summary>
    /// <param name="changePasswordDto">Password change details</param>
    /// <returns>Success message</returns>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        if (userId == 0)
            return Unauthorized();

        await _authService.ChangePasswordAsync(userId, changePasswordDto);

        return Ok(new { message = "Password changed successfully" });
    }

    /// <summary>
    /// Complete forced password change for invited users
    /// </summary>
    [HttpPost("complete-password-change")]
    [Authorize]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompletePasswordChange([FromBody] CompletePasswordChangeDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        if (userId == 0)
            return Unauthorized();

        var response = await _authService.CompletePasswordChangeAsync(userId, dto);
        return Ok(response);
    }

    /// <summary>
    /// Send a verification OTP to the current user
    /// </summary>
    [HttpPost("verification/send")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendVerificationOtp([FromBody] SendVerificationOtpDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        if (userId == 0)
            return Unauthorized();

        await _verificationService.SendVerificationOtpAsync(userId, dto.Channel);
        return Ok(new { message = "Verification code sent" });
    }

    /// <summary>
    /// Verify OTP for the current user and return refreshed auth payload
    /// </summary>
    [HttpPost("verification/verify")]
    [Authorize]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        if (userId == 0)
            return Unauthorized();

        await _verificationService.VerifyOtpAsync(userId, dto.Code);
        var response = await _authService.RefreshAuthAsync(userId);
        return Ok(response);
    }

    /// <summary>
    /// Update user status (activate/suspend)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="status">New status</param>
    /// <returns>Success message</returns>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "PlatformAdmin,Landlord")]
    [Authorize(Policy = Policies.RequireVerifiedUser)]
    [Authorize(Policy = Policies.RequireActiveOrganization)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UserStatus status)
    {
        await _authService.UpdateUserStatusAsync(id, status);
        return Ok(new { message = $"User status updated to {status}" });
    }

    /// <summary>
    /// Delete user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "PlatformAdmin")]
    [Authorize(Policy = Policies.RequireVerifiedUser)]
    [Authorize(Policy = Policies.RequireActiveOrganization)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _authService.DeleteUserAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Request password reset (sends email with reset link)
    /// </summary>
    /// <param name="forgotPasswordDto">Email address</param>
    /// <returns>Success message</returns>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        await _authService.RequestPasswordResetAsync(forgotPasswordDto.Email);

        // Always return success message (security best practice - don't reveal if email exists)
        return Ok(new { message = "If the email exists in our system, a password reset link has been sent." });
    }

    /// <summary>
    /// Reset password using a valid token
    /// </summary>
    /// <param name="resetPasswordDto">Reset password details</param>
    /// <returns>Success message</returns>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        await _authService.ResetPasswordAsync(resetPasswordDto);
        return Ok(new { message = "Password reset successful. You can now login with your new password." });
    }

    /// <summary>
    /// Verify email address using a valid token
    /// </summary>
    /// <param name="token">Email verification token</param>
    /// <returns>Success message</returns>
    [HttpGet("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new { message = "Token is required" });
        }

        await _authService.VerifyEmailAsync(token);
        return Ok(new { message = "Email verified successfully. You can now login." });
    }

    /// <summary>
    /// Resend email verification link
    /// </summary>
    /// <param name="forgotPasswordDto">Email address</param>
    /// <returns>Success message</returns>
    [HttpPost("resend-verification")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendEmailVerification([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        await _authService.ResendEmailVerificationAsync(forgotPasswordDto.Email);

        // Always return success message (security best practice - don't reveal if email exists)
        return Ok(new { message = "If the email exists in our system, a verification link has been sent." });
    }
}

