using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.DTOs.Tenants;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.API.Controllers;

/// <summary>
/// Tenant application management for landlords
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires authentication
public class TenantApplicationsController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly ILogger<TenantApplicationsController> _logger;

    public TenantApplicationsController(
        ITenantService tenantService,
        ILogger<TenantApplicationsController> logger)
    {
        _tenantService = tenantService;
        _logger = logger;
    }

    /// <summary>
    /// Get all pending tenant applications (Landlord/SystemAdmin only)
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "SystemAdmin,Landlord,Caretaker")]
    [ProducesResponseType(typeof(IEnumerable<TenantApplicationResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingApplications()
    {
        var result = await _tenantService.GetPendingApplicationsAsync();

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Get specific application details (Landlord/SystemAdmin only)
    /// </summary>
    [HttpGet("{applicationId:int}")]
    [Authorize(Roles = "SystemAdmin,Landlord,Caretaker")]
    [ProducesResponseType(typeof(TenantApplicationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetApplicationById(int applicationId)
    {
        var result = await _tenantService.GetApplicationByIdAsync(applicationId);

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        if (result.Errors.Any(e => e.Contains("not found")))
        {
            return NotFound(result.Errors);
        }

        return StatusCode(StatusCodes.Status403Forbidden, result.Errors);
    }

    /// <summary>
    /// Approve or reject a tenant application (Landlord/SystemAdmin only)
    /// </summary>
    [HttpPost("{applicationId:int}/review")]
    [Authorize(Roles = "SystemAdmin,Landlord")]
    [ProducesResponseType(typeof(TenantApplicationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReviewApplication(
        int applicationId,
        [FromBody] ReviewApplicationDto reviewDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _tenantService.ReviewApplicationAsync(applicationId, reviewDto);

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        if (result.Errors.Any(e => e.Contains("not found")))
        {
            return NotFound(result.Errors);
        }

        if (result.Errors.Any(e => e.Contains("permission")))
        {
            return StatusCode(StatusCodes.Status403Forbidden, result.Errors);
        }

        return BadRequest(result.Errors);
    }
}
