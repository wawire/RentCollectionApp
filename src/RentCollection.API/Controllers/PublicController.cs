using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.DTOs.Public;
using RentCollection.Application.DTOs.Tenants;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.API.Controllers;

/// <summary>
/// Public API endpoints (no authentication required)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PublicController : ControllerBase
{
    private readonly IPublicListingService _publicListingService;
    private readonly ITenantService _tenantService;
    private readonly ILogger<PublicController> _logger;

    public PublicController(
        IPublicListingService publicListingService,
        ITenantService tenantService,
        ILogger<PublicController> logger)
    {
        _publicListingService = publicListingService;
        _tenantService = tenantService;
        _logger = logger;
    }

    /// <summary>
    /// Get all properties with vacant units
    /// </summary>
    [HttpGet("properties")]
    [ProducesResponseType(typeof(IEnumerable<PublicPropertyListingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublicProperties()
    {
        var result = await _publicListingService.GetPublicPropertiesAsync();

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Get specific property details with available units
    /// </summary>
    [HttpGet("properties/{propertyId:int}")]
    [ProducesResponseType(typeof(PublicPropertyListingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPublicPropertyById(int propertyId)
    {
        var result = await _publicListingService.GetPublicPropertyByIdAsync(propertyId);

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return NotFound(result.Errors);
    }

    /// <summary>
    /// Get all vacant units across all properties
    /// </summary>
    [HttpGet("units/vacant")]
    [ProducesResponseType(typeof(IEnumerable<PublicUnitListingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVacantUnits()
    {
        var result = await _publicListingService.GetVacantUnitsAsync();

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Get vacant units for a specific property
    /// </summary>
    [HttpGet("properties/{propertyId:int}/units/vacant")]
    [ProducesResponseType(typeof(IEnumerable<PublicUnitListingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVacantUnitsByProperty(int propertyId)
    {
        var result = await _publicListingService.GetVacantUnitsByPropertyAsync(propertyId);

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return NotFound(result.Errors);
    }

    /// <summary>
    /// Get details of a specific unit
    /// </summary>
    [HttpGet("units/{unitId:int}")]
    [ProducesResponseType(typeof(PublicUnitListingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUnitDetails(int unitId)
    {
        var result = await _publicListingService.GetUnitDetailsAsync(unitId);

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return NotFound(result.Errors);
    }

    /// <summary>
    /// Submit a tenant application (no authentication required)
    /// </summary>
    [HttpPost("applications")]
    [ProducesResponseType(typeof(TenantApplicationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitApplication([FromBody] TenantApplicationDto applicationDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _tenantService.SubmitApplicationAsync(applicationDto);

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetUnitDetails),
                new { unitId = result.Data!.UnitId },
                result.Data);
        }

        return BadRequest(result.Errors);
    }
}
