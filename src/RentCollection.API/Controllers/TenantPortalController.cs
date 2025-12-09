using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.DTOs.Payments;
using RentCollection.Application.DTOs.Tenants;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.API.Controllers;

/// <summary>
/// Tenant portal for tenants to view their own data
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Tenant")] // Only tenants can access
public class TenantPortalController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly ITenantPortalService _tenantPortalService;
    private readonly ILogger<TenantPortalController> _logger;

    public TenantPortalController(
        ITenantService tenantService,
        ITenantPortalService tenantPortalService,
        ILogger<TenantPortalController> logger)
    {
        _tenantService = tenantService;
        _tenantPortalService = tenantPortalService;
        _logger = logger;
    }

    /// <summary>
    /// Get current tenant's own information (lease, unit, property details)
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(TenantPortalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyInfo()
    {
        var result = await _tenantService.GetMyTenantInfoAsync();

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return NotFound(result.Errors);
    }

    /// <summary>
    /// Get current tenant's payment history
    /// </summary>
    [HttpGet("me/payments")]
    [ProducesResponseType(typeof(IEnumerable<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyPayments()
    {
        var result = await _tenantService.GetMyPaymentsAsync();

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return NotFound(result.Errors);
    }

    /// <summary>
    /// Get tenant dashboard with key metrics
    /// Shows current balance, next payment, recent payments, documents, etc.
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _tenantPortalService.GetMyDashboardAsync();

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get comprehensive lease information
    /// Includes property details, unit details, lease terms, payment instructions
    /// </summary>
    [HttpGet("lease-info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLeaseInfo()
    {
        var result = await _tenantPortalService.GetMyLeaseInfoAsync();

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}
