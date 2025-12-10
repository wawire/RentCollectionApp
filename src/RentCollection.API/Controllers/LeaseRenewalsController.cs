using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.DTOs.LeaseRenewals;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Security;

namespace RentCollection.API.Controllers;

/// <summary>
/// Lease renewal management endpoints for landlords and tenants
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LeaseRenewalsController : ControllerBase
{
    private readonly ILeaseRenewalService _leaseRenewalService;
    private readonly ILogger<LeaseRenewalsController> _logger;

    public LeaseRenewalsController(
        ILeaseRenewalService leaseRenewalService,
        ILogger<LeaseRenewalsController> logger)
    {
        _leaseRenewalService = leaseRenewalService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new lease renewal request (Landlords only)
    /// </summary>
    /// <param name="dto">Lease renewal details</param>
    /// <returns>Created lease renewal</returns>
    [HttpPost]
    [PermissionAuthorize(Permission.CreateLeaseRenewal)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateLeaseRenewal([FromBody] CreateLeaseRenewalDto dto)
    {
        var result = await _leaseRenewalService.CreateLeaseRenewalAsync(dto);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get all lease renewals (filtered by RBAC)
    /// </summary>
    /// <returns>List of lease renewals</returns>
    [HttpGet]
    [PermissionAuthorize(Permission.ViewLeaseRenewals)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllLeaseRenewals()
    {
        var result = await _leaseRenewalService.GetAllLeaseRenewalsAsync();

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get a specific lease renewal by ID
    /// </summary>
    /// <param name="id">Lease renewal ID</param>
    /// <returns>Lease renewal details</returns>
    [HttpGet("{id}")]
    [PermissionAuthorize(Permission.ViewLeaseRenewals)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLeaseRenewalById(int id)
    {
        var result = await _leaseRenewalService.GetLeaseRenewalByIdAsync(id);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get current tenant's lease renewals
    /// </summary>
    /// <returns>List of tenant's lease renewals</returns>
    [HttpGet("my-renewals")]
    [Authorize(Roles = "Tenant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMyLeaseRenewals()
    {
        var result = await _leaseRenewalService.GetMyLeaseRenewalsAsync();

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get lease renewals for a specific property
    /// </summary>
    /// <param name="propertyId">Property ID</param>
    /// <returns>List of lease renewals for the property</returns>
    [HttpGet("property/{propertyId}")]
    [PermissionAuthorize(Permission.ViewLeaseRenewals)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLeaseRenewalsByProperty(int propertyId)
    {
        var result = await _leaseRenewalService.GetLeaseRenewalsByPropertyIdAsync(propertyId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get lease renewals by status
    /// </summary>
    /// <param name="status">Lease renewal status</param>
    /// <returns>List of lease renewals with the specified status</returns>
    [HttpGet("status/{status}")]
    [PermissionAuthorize(Permission.ViewLeaseRenewals)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLeaseRenewalsByStatus(LeaseRenewalStatus status)
    {
        var result = await _leaseRenewalService.GetLeaseRenewalsByStatusAsync(status);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get leases expiring soon (within specified days)
    /// </summary>
    /// <param name="daysUntilExpiry">Number of days until expiry (default: 60)</param>
    /// <returns>List of leases expiring soon</returns>
    [HttpGet("expiring-soon")]
    [PermissionAuthorize(Permission.ViewLeaseRenewals)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetExpiringSoon([FromQuery] int daysUntilExpiry = 60)
    {
        var result = await _leaseRenewalService.GetExpiringSoonAsync(daysUntilExpiry);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Update a lease renewal
    /// </summary>
    /// <param name="id">Lease renewal ID</param>
    /// <param name="dto">Updated lease renewal details</param>
    /// <returns>Updated lease renewal</returns>
    [HttpPut("{id}")]
    [PermissionAuthorize(Permission.UpdateLeaseRenewal)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLeaseRenewal(int id, [FromBody] UpdateLeaseRenewalDto dto)
    {
        var result = await _leaseRenewalService.UpdateLeaseRenewalAsync(id, dto);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Tenant responds to a lease renewal (accept or reject)
    /// </summary>
    /// <param name="id">Lease renewal ID</param>
    /// <param name="dto">Tenant response (accept/reject)</param>
    /// <returns>Updated lease renewal</returns>
    [HttpPost("{id}/respond")]
    [PermissionAuthorize(Permission.RespondToLeaseRenewal)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TenantRespond(int id, [FromBody] TenantResponseDto dto)
    {
        var result = await _leaseRenewalService.TenantRespondAsync(id, dto);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Landlord approves a lease renewal
    /// </summary>
    /// <param name="id">Lease renewal ID</param>
    /// <returns>Updated lease renewal</returns>
    [HttpPost("{id}/approve")]
    [PermissionAuthorize(Permission.ApproveLeaseRenewal)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LandlordApprove(int id)
    {
        var result = await _leaseRenewalService.LandlordApproveAsync(id);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Landlord rejects a lease renewal
    /// </summary>
    /// <param name="id">Lease renewal ID</param>
    /// <param name="request">Rejection details</param>
    /// <returns>Updated lease renewal</returns>
    [HttpPost("{id}/reject")]
    [PermissionAuthorize(Permission.ApproveLeaseRenewal)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LandlordReject(int id, [FromBody] RejectLeaseRenewalDto request)
    {
        var result = await _leaseRenewalService.LandlordRejectAsync(id, request.Reason);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Complete a lease renewal (updates tenant's lease)
    /// </summary>
    /// <param name="id">Lease renewal ID</param>
    /// <returns>Updated lease renewal</returns>
    [HttpPost("{id}/complete")]
    [PermissionAuthorize(Permission.ApproveLeaseRenewal)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteRenewal(int id)
    {
        var result = await _leaseRenewalService.CompleteRenewalAsync(id);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete a lease renewal
    /// </summary>
    /// <param name="id">Lease renewal ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [PermissionAuthorize(Permission.DeleteLeaseRenewal)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLeaseRenewal(int id)
    {
        var result = await _leaseRenewalService.DeleteLeaseRenewalAsync(id);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}

/// <summary>
/// DTO for rejecting a lease renewal
/// </summary>
public class RejectLeaseRenewalDto
{
    public string Reason { get; set; } = string.Empty;
}
