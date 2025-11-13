using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.DTOs.Auth;
using RentCollection.Application.Services.Interfaces;
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
    /// User login
    /// </summary>
    /// <param name="loginDto">Login credentials (username and password)</param>
    /// <returns>Access token, refresh token, and user information</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.LoginAsync(loginDto);

        if (!result.IsSuccess)
            return Unauthorized(result);

        return Ok(result);
    }

    /// <summary>
    /// User registration
    /// </summary>
    /// <param name="registerDto">Registration information</param>
    /// <returns>Access token, refresh token, and user information</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RegisterAsync(registerDto);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    /// <param name="refreshTokenDto">Refresh token</param>
    /// <returns>New access token and refresh token</returns>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);

        if (!result.IsSuccess)
            return Unauthorized(result);

        return Ok(result);
    }

    /// <summary>
    /// User logout
    /// </summary>
    /// <returns>Success message</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Invalid user token" });
        }

        var result = await _authService.LogoutAsync(userId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    /// <returns>Current user details</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Invalid user token" });
        }

        var result = await _authService.GetUserByIdAsync(userId);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    /// <returns>List of all users</returns>
    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _authService.GetAllUsersAsync();

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}
