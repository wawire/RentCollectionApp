using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.DTOs.Tenants;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.API.Controllers;

/// <summary>
/// Tenants management endpoints
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TenantsController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly ILogger<TenantsController> _logger;

    public TenantsController(ITenantService tenantService, ILogger<TenantsController> logger)
    {
        _tenantService = tenantService;
        _logger = logger;
    }

    /// <summary>
    /// Get all tenants
    /// </summary>
    /// <returns>List of all tenants</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _tenantService.GetAllTenantsAsync();

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get tenant by ID
    /// </summary>
    /// <param name="id">Tenant ID</param>
    /// <returns>Tenant details including payment history</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _tenantService.GetTenantByIdAsync(id);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get tenants by unit ID
    /// </summary>
    /// <param name="unitId">Unit ID</param>
    /// <returns>List of tenants for the specified unit</returns>
    [HttpGet("unit/{unitId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUnitId(int unitId)
    {
        var result = await _tenantService.GetTenantsByUnitIdAsync(unitId);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get all tenants in occupied units (active tenants only)
    /// </summary>
    /// <returns>List of active tenants in occupied units</returns>
    [HttpGet("occupied")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOccupiedTenants()
    {
        var result = await _tenantService.GetAllTenantsAsync();

        if (!result.IsSuccess)
            return BadRequest(result);

        // Filter to only active tenants (IsActive = true means tenant is currently occupying the unit)
        var occupiedTenants = result.Data?.Where(t => t.IsActive).ToList();

        return Ok(new { isSuccess = true, data = occupiedTenants });
    }

    /// <summary>
    /// Create a new tenant
    /// </summary>
    /// <param name="createDto">Tenant creation data</param>
    /// <returns>Created tenant</returns>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin,Landlord,Caretaker")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTenantDto createDto)
    {
        var result = await _tenantService.CreateTenantAsync(createDto);

        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update an existing tenant
    /// </summary>
    /// <param name="id">Tenant ID</param>
    /// <param name="updateDto">Tenant update data</param>
    /// <returns>Updated tenant</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "SystemAdmin,Landlord,Caretaker")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTenantDto updateDto)
    {
        var result = await _tenantService.UpdateTenantAsync(id, updateDto);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete a tenant
    /// </summary>
    /// <param name="id">Tenant ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SystemAdmin,Landlord")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _tenantService.DeleteTenantAsync(id);

        if (!result.IsSuccess)
            return BadRequest(result);

        return NoContent();
    }
}
