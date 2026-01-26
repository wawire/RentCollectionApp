using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.DTOs.Utilities;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MeterReadingsController : ControllerBase
{
    private readonly IMeterReadingService _meterReadingService;

    public MeterReadingsController(IMeterReadingService meterReadingService)
    {
        _meterReadingService = meterReadingService;
    }

    [HttpGet]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager,Caretaker,Accountant")]
    public async Task<IActionResult> GetMeterReadings(
        [FromQuery] int? propertyId = null,
        [FromQuery] int? unitId = null,
        [FromQuery] int? utilityConfigId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var result = await _meterReadingService.GetMeterReadingsAsync(propertyId, unitId, utilityConfigId, startDate, endDate);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager,Caretaker")]
    public async Task<IActionResult> CreateMeterReading([FromBody] CreateMeterReadingDto dto)
    {
        var result = await _meterReadingService.CreateMeterReadingAsync(dto);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

