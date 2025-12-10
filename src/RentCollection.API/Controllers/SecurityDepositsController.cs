using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.DTOs.SecurityDeposits;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Enums;
using System.Security.Claims;

namespace RentCollection.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SecurityDepositsController : ControllerBase
{
    private readonly ISecurityDepositService _securityDepositService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<SecurityDepositsController> _logger;

    public SecurityDepositsController(
        ISecurityDepositService securityDepositService,
        IAuthorizationService authorizationService,
        ILogger<SecurityDepositsController> logger)
    {
        _securityDepositService = securityDepositService;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all security deposits (Landlord/Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "Permission.ViewSecurityDeposits")]
    public async Task<IActionResult> GetAllDeposits()
    {
        var result = await _securityDepositService.GetAllDepositsAsync();

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get security deposit balance for specific tenant
    /// </summary>
    [HttpGet("tenant/{tenantId}/balance")]
    public async Task<IActionResult> GetDepositBalance(int tenantId)
    {
        // Tenants can only view their own deposit
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        var userTenantId = User.FindFirst("TenantId")?.Value;

        if (userRole == "Tenant" && userTenantId != tenantId.ToString())
            return Forbid();

        // Non-tenants need permission
        if (userRole != "Tenant")
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, "Permission.ViewSecurityDeposits");
            if (!authResult.Succeeded)
                return Forbid();
        }

        var result = await _securityDepositService.GetDepositBalanceAsync(tenantId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get transaction history for tenant's security deposit
    /// </summary>
    [HttpGet("tenant/{tenantId}/transactions")]
    public async Task<IActionResult> GetTransactionHistory(
        int tenantId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        // Tenants can only view their own transactions
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        var userTenantId = User.FindFirst("TenantId")?.Value;

        if (userRole == "Tenant" && userTenantId != tenantId.ToString())
            return Forbid();

        // Non-tenants need permission
        if (userRole != "Tenant")
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, "Permission.ViewSecurityDeposits");
            if (!authResult.Succeeded)
                return Forbid();
        }

        var result = await _securityDepositService.GetTransactionHistoryAsync(tenantId, startDate, endDate);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Record initial security deposit payment (Landlord/Admin only)
    /// </summary>
    [HttpPost("tenant/{tenantId}/record")]
    [Authorize(Policy = "Permission.RecordSecurityDeposit")]
    public async Task<IActionResult> RecordDepositPayment(int tenantId, [FromBody] RecordSecurityDepositDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _securityDepositService.RecordDepositPaymentAsync(tenantId, dto, userId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return CreatedAtAction(nameof(GetDepositBalance), new { tenantId }, result.Data);
    }

    /// <summary>
    /// Deduct from security deposit (Landlord/Admin only)
    /// </summary>
    [HttpPost("tenant/{tenantId}/deduct")]
    [Authorize(Policy = "Permission.DeductSecurityDeposit")]
    public async Task<IActionResult> DeductFromDeposit(int tenantId, [FromBody] DeductSecurityDepositDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _securityDepositService.DeductFromDepositAsync(tenantId, dto, userId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Refund security deposit to tenant (Landlord/Admin only)
    /// </summary>
    [HttpPost("tenant/{tenantId}/refund")]
    [Authorize(Policy = "Permission.RefundSecurityDeposit")]
    public async Task<IActionResult> RefundDeposit(int tenantId, [FromBody] RefundSecurityDepositDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _securityDepositService.RefundDepositAsync(tenantId, dto, userId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Data);
    }
}
