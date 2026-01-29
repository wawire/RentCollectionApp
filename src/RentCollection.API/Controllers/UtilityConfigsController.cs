using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.DTOs.Utilities;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UtilityConfigsController : ControllerBase
{
    private readonly IUtilityConfigService _utilityConfigService;

    public UtilityConfigsController(IUtilityConfigService utilityConfigService)
    {
        _utilityConfigService = utilityConfigService;
    }

    [HttpGet]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager,Accountant")]
    public async Task<IActionResult> GetUtilityConfigs(
        [FromQuery] int? propertyId = null,
        [FromQuery] int? unitId = null,
        [FromQuery] bool includeInactive = false)
    {
        var result = await _utilityConfigService.GetUtilityConfigsAsync(propertyId, unitId, includeInactive);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager")]
    public async Task<IActionResult> CreateUtilityConfig([FromBody] CreateUtilityConfigDto dto)
    {
        var result = await _utilityConfigService.CreateUtilityConfigAsync(dto);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager")]
    public async Task<IActionResult> UpdateUtilityConfig(int id, [FromBody] UpdateUtilityConfigDto dto)
    {
        var result = await _utilityConfigService.UpdateUtilityConfigAsync(id, dto);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

