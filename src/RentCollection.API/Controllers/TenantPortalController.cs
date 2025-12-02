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
    private readonly ILogger<TenantPortalController> _logger;

    public TenantPortalController(
        ITenantService tenantService,
        ILogger<TenantPortalController> logger)
    {
        _tenantService = tenantService;
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

        return NotFound(result.Error);
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

        return NotFound(result.Error);
    }
}
