using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.Authorization;
using RentCollection.Application.DTOs.Payments;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Enums;

namespace RentCollection.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Authorize(Policy = Policies.RequireVerifiedUser)]
[Authorize(Policy = Policies.RequireActiveOrganization)]
public class UnmatchedPaymentsController : ControllerBase
{
    private readonly IUnmatchedPaymentService _unmatchedPaymentService;

    public UnmatchedPaymentsController(IUnmatchedPaymentService unmatchedPaymentService)
    {
        _unmatchedPaymentService = unmatchedPaymentService;
    }

    [HttpGet]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager,Accountant")]
    public async Task<IActionResult> Get([FromQuery] UnmatchedPaymentStatus? status = null)
    {
        var result = await _unmatchedPaymentService.GetUnmatchedPaymentsAsync(status);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager,Accountant")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateUnmatchedPaymentStatusDto dto)
    {
        var result = await _unmatchedPaymentService.UpdateStatusAsync(id, dto.Status);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("{id}/resolve")]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager,Accountant")]
    public async Task<IActionResult> Resolve(int id, [FromBody] ResolveUnmatchedPaymentDto dto)
    {
        var result = await _unmatchedPaymentService.ResolveUnmatchedPaymentAsync(id, dto);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

