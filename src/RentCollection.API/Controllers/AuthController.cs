using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.DTOs.Auth;
using RentCollection.Application.Services.Auth;
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
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
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
    [Authorize(Roles = "SystemAdmin,Landlord")] // Only SystemAdmin and Landlord can create users
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
    [Authorize(Roles = "SystemAdmin,Landlord")]
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
    [Authorize(Roles = "SystemAdmin,Landlord")]
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
    /// Update user status (activate/suspend)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="status">New status</param>
    /// <returns>Success message</returns>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "SystemAdmin,Landlord")]
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
    [Authorize(Roles = "SystemAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _authService.DeleteUserAsync(id);
        return NoContent();
    }
}
