using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.Authorization;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "PlatformAdmin,Landlord,Manager")]
[Authorize(Policy = Policies.RequireVerifiedUser)]
[Authorize(Policy = Policies.RequireActiveOrganization)]
public class ReconciliationController : ControllerBase
{
    private readonly IPaymentAllocationService _allocationService;

    public ReconciliationController(IPaymentAllocationService allocationService)
    {
        _allocationService = allocationService;
    }

    [HttpPost("payments/{paymentId}/allocate")]
    public async Task<IActionResult> AllocatePayment(int paymentId, [FromBody] AllocatePaymentDto dto)
    {
        var result = await _allocationService.AllocatePaymentAsync(paymentId, dto.InvoiceId, dto.Amount);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

public class AllocatePaymentDto
{
    public int? InvoiceId { get; set; }
    public decimal? Amount { get; set; }
}

