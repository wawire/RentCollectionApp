using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.DTOs.MaintenanceRequests;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Enums;
using RentCollection.Application.Authorization;

namespace RentCollection.API.Controllers;

/// <summary>
/// Maintenance request management endpoints for tenants, landlords, and caretakers
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MaintenanceRequestsController : ControllerBase
{
    private readonly IMaintenanceRequestService _maintenanceRequestService;
    private readonly ILogger<MaintenanceRequestsController> _logger;

    public MaintenanceRequestsController(
        IMaintenanceRequestService maintenanceRequestService,
        ILogger<MaintenanceRequestsController> logger)
    {
        _maintenanceRequestService = maintenanceRequestService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new maintenance request (Tenants only)
    /// </summary>
    /// <param name="dto">Maintenance request details with optional photos (max 5, 5MB each)</param>
    /// <returns>Created maintenance request</returns>
    [HttpPost]
    [PermissionAuthorize(Permission.CreateMaintenanceRequest)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMaintenanceRequest([FromForm] CreateMaintenanceRequestDto dto)
    {
        var result = await _maintenanceRequestService.CreateMaintenanceRequestAsync(dto);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get all maintenance requests (filtered by RBAC)
    /// </summary>
    /// <returns>List of maintenance requests</returns>
    [HttpGet]
    [PermissionAuthorize(Permission.ViewMaintenanceRequests)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllMaintenanceRequests()
    {
        var result = await _maintenanceRequestService.GetAllMaintenanceRequestsAsync();

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get a specific maintenance request by ID
    /// </summary>
    /// <param name="id">Maintenance request ID</param>
    /// <returns>Maintenance request details</returns>
    [HttpGet("{id}")]
    [PermissionAuthorize(Permission.ViewMaintenanceRequests)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMaintenanceRequestById(int id)
    {
        var result = await _maintenanceRequestService.GetMaintenanceRequestByIdAsync(id);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get current tenant's maintenance requests
    /// </summary>
    /// <returns>List of tenant's maintenance requests</returns>
    [HttpGet("my-requests")]
    [Authorize(Roles = "Tenant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMyMaintenanceRequests()
    {
        var result = await _maintenanceRequestService.GetMyMaintenanceRequestsAsync();

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get maintenance requests for a specific property
    /// </summary>
    /// <param name="propertyId">Property ID</param>
    /// <returns>List of maintenance requests for the property</returns>
    [HttpGet("property/{propertyId}")]
    [PermissionAuthorize(Permission.ViewMaintenanceRequests)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMaintenanceRequestsByProperty(int propertyId)
    {
        var result = await _maintenanceRequestService.GetMaintenanceRequestsByPropertyIdAsync(propertyId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get maintenance requests by status
    /// </summary>
    /// <param name="status">Maintenance request status</param>
    /// <returns>List of maintenance requests with the specified status</returns>
    [HttpGet("status/{status}")]
    [PermissionAuthorize(Permission.ViewMaintenanceRequests)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMaintenanceRequestsByStatus(MaintenanceRequestStatus status)
    {
        var result = await _maintenanceRequestService.GetMaintenanceRequestsByStatusAsync(status);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get maintenance requests assigned to current caretaker
    /// </summary>
    /// <returns>List of assigned maintenance requests</returns>
    [HttpGet("assigned-to-me")]
    [Authorize(Roles = "Caretaker")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAssignedMaintenanceRequests()
    {
        var result = await _maintenanceRequestService.GetAssignedMaintenanceRequestsAsync();

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Update a maintenance request
    /// </summary>
    /// <param name="id">Maintenance request ID</param>
    /// <param name="dto">Updated maintenance request details</param>
    /// <returns>Updated maintenance request</returns>
    [HttpPut("{id}")]
    [PermissionAuthorize(Permission.UpdateMaintenanceRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMaintenanceRequest(int id, [FromBody] UpdateMaintenanceRequestDto dto)
    {
        var result = await _maintenanceRequestService.UpdateMaintenanceRequestAsync(id, dto);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Assign a maintenance request to a caretaker
    /// </summary>
    /// <param name="id">Maintenance request ID</param>
    /// <param name="request">Assignment request containing caretaker ID</param>
    /// <returns>Updated maintenance request</returns>
    [HttpPost("{id}/assign")]
    [PermissionAuthorize(Permission.AssignMaintenanceRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignMaintenanceRequest(int id, [FromBody] AssignMaintenanceRequestDto request)
    {
        var result = await _maintenanceRequestService.AssignMaintenanceRequestAsync(id, request.CaretakerId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Complete a maintenance request
    /// </summary>
    /// <param name="id">Maintenance request ID</param>
    /// <param name="request">Completion details including actual cost and notes</param>
    /// <returns>Updated maintenance request</returns>
    [HttpPost("{id}/complete")]
    [PermissionAuthorize(Permission.CompleteMaintenanceRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteMaintenanceRequest(int id, [FromBody] CompleteMaintenanceRequestDto request)
    {
        var result = await _maintenanceRequestService.CompleteMaintenanceRequestAsync(
            id,
            request.ActualCost,
            request.Notes);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete a maintenance request
    /// </summary>
    /// <param name="id">Maintenance request ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [PermissionAuthorize(Permission.DeleteMaintenanceRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMaintenanceRequest(int id)
    {
        var result = await _maintenanceRequestService.DeleteMaintenanceRequestAsync(id);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}

/// <summary>
/// DTO for assigning a maintenance request
/// </summary>
public class AssignMaintenanceRequestDto
{
    public int CaretakerId { get; set; }
}

/// <summary>
/// DTO for completing a maintenance request
/// </summary>
public class CompleteMaintenanceRequestDto
{
    public decimal ActualCost { get; set; }
    public string? Notes { get; set; }
}
