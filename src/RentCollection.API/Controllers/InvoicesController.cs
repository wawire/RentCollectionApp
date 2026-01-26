using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.Authorization;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Authorize(Policy = Policies.RequireVerifiedUser)]
[Authorize(Policy = Policies.RequireActiveOrganization)]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<InvoicesController> _logger;

    public InvoicesController(
        IInvoiceService invoiceService,
        ICurrentUserService currentUserService,
        ILogger<InvoicesController> logger)
    {
        _invoiceService = invoiceService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager,Caretaker,Accountant,Tenant")]
    public async Task<IActionResult> GetInvoices(
        [FromQuery] int? propertyId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var result = await _invoiceService.GetInvoicesAsync(propertyId, startDate, endDate);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager,Caretaker,Accountant,Tenant")]
    public async Task<IActionResult> GetInvoiceById(int id)
    {
        var result = await _invoiceService.GetInvoiceByIdAsync(id);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("tenant/{tenantId}")]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager,Caretaker,Accountant,Tenant")]
    public async Task<IActionResult> GetInvoicesByTenant(int tenantId)
    {
        try
        {
            var result = await _invoiceService.GetInvoicesByTenantAsync(tenantId);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invoices for tenant {TenantId}", tenantId);
            return BadRequest(new { message = "Failed to retrieve invoices" });
        }
    }

    [HttpGet("me")]
    [Authorize(Roles = "Tenant")]
    public async Task<IActionResult> GetMyInvoices()
    {
        var tenantId = _currentUserService.TenantId;
        if (!tenantId.HasValue)
        {
            return BadRequest(new { message = "User is not a tenant" });
        }

        var result = await _invoiceService.GetInvoicesByTenantAsync(tenantId.Value);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("generate")]
    [Authorize(Roles = "PlatformAdmin,Landlord")]
    public async Task<IActionResult> GenerateInvoices([FromQuery] int year, [FromQuery] int month)
    {
        if (year < 2000 || month < 1 || month > 12)
        {
            return BadRequest(new { message = "Invalid year or month" });
        }

        var result = await _invoiceService.GenerateMonthlyInvoicesAsync(year, month);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

