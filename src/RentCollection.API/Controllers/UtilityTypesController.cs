using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.DTOs.Utilities;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UtilityTypesController : ControllerBase
{
    private readonly IUtilityTypeService _utilityTypeService;

    public UtilityTypesController(IUtilityTypeService utilityTypeService)
    {
        _utilityTypeService = utilityTypeService;
    }

    [HttpGet]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager")]
    public async Task<IActionResult> GetUtilityTypes([FromQuery] bool includeInactive = false)
    {
        var result = await _utilityTypeService.GetUtilityTypesAsync(includeInactive);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "PlatformAdmin")]
    public async Task<IActionResult> CreateUtilityType([FromBody] CreateUtilityTypeDto dto)
    {
        var result = await _utilityTypeService.CreateUtilityTypeAsync(dto);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

