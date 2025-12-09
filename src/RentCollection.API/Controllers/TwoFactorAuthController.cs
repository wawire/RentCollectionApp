using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.DTOs.Auth;
using RentCollection.Application.Services.Auth;
using System.Security.Claims;

namespace RentCollection.API.Controllers;

/// <summary>
/// Two-Factor Authentication endpoints
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TwoFactorAuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<TwoFactorAuthController> _logger;

    public TwoFactorAuthController(
        IAuthService authService,
        ILogger<TwoFactorAuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Setup 2FA for the current user
    /// Returns QR code and secret for authenticator apps
    /// </summary>
    /// <returns>Setup information including QR code URI and secret</returns>
    [HttpPost("setup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Setup()
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _authService.Setup2FAAsync(userId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up 2FA");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Enable 2FA after verifying setup with a code from authenticator app
    /// </summary>
    /// <param name="dto">Verification code from authenticator app</param>
    /// <returns>Success message</returns>
    [HttpPost("enable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Enable([FromBody] Enable2FADto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _authService.Enable2FAAsync(userId, dto);

            return Ok(new { message = "Two-factor authentication enabled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling 2FA");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Disable 2FA (requires password verification)
    /// </summary>
    /// <param name="dto">Password for verification</param>
    /// <returns>Success message</returns>
    [HttpPost("disable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Disable([FromBody] Disable2FADto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _authService.Disable2FAAsync(userId, dto);

            return Ok(new { message = "Two-factor authentication disabled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling 2FA");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Verify 2FA code during login (no authentication required)
    /// This is called after initial username/password verification
    /// </summary>
    /// <param name="dto">Email/phone and verification code</param>
    /// <returns>JWT token if code is valid</returns>
    [AllowAnonymous]
    [HttpPost("verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Verify([FromBody] Verify2FACodeDto dto)
    {
        try
        {
            var result = await _authService.Verify2FACodeAsync(dto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("2FA verification failed: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying 2FA code");
            return BadRequest(new { message = ex.Message });
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return userId;
    }
}
