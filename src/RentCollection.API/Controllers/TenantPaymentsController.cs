using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.Authorization;
using RentCollection.Application.DTOs.Payments;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.API.Controllers;

/// <summary>
/// Tenant-specific payment endpoints
/// </summary>
[Authorize(Roles = "Tenant")]
[Authorize(Policy = Policies.RequireVerifiedUser)]
[Authorize(Policy = Policies.RequirePasswordChangeComplete)]
[Authorize(Policy = Policies.RequireActiveOrganization)]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TenantPaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IMPesaService _mpesaService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<TenantPaymentsController> _logger;

    public TenantPaymentsController(
        IPaymentService paymentService,
        IMPesaService mpesaService,
        ICurrentUserService currentUserService,
        ILogger<TenantPaymentsController> logger)
    {
        _paymentService = paymentService;
        _mpesaService = mpesaService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Get payment instructions for the current tenant
    /// </summary>
    /// <returns>Payment instructions including landlord account details</returns>
    [HttpGet("instructions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentInstructions()
    {
        var tenantId = _currentUserService.TenantId;
        if (tenantId == null)
            return BadRequest(new { message = "User is not a tenant" });

        var result = await _paymentService.GetPaymentInstructionsAsync(tenantId.Value);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Record a payment made by the current tenant
    /// </summary>
    /// <param name="dto">Payment recording data</param>
    /// <returns>Created payment record (Pending status)</returns>
    [HttpPost("record")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecordPayment([FromBody] TenantRecordPaymentDto dto)
    {
        var tenantId = _currentUserService.TenantId;
        if (tenantId == null)
            return BadRequest(new { message = "User is not a tenant" });

        var result = await _paymentService.RecordTenantPaymentAsync(tenantId.Value, dto);

        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(
            nameof(GetPaymentHistory),
            new { },
            result);
    }

    /// <summary>
    /// Get payment history for the current tenant
    /// </summary>
    /// <returns>List of all payments made by the tenant</returns>
    [HttpGet("history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentHistory()
    {
        var tenantId = _currentUserService.TenantId;
        if (tenantId == null)
            return BadRequest(new { message = "User is not a tenant" });

        var result = await _paymentService.GetPaymentsByTenantIdAsync(tenantId.Value);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Initiate M-Pesa STK Push for rent payment
    /// </summary>
    /// <param name="dto">STK Push details</param>
    /// <returns>STK Push response</returns>
    [HttpPost("stk-push")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InitiateStkPush([FromBody] InitiateStkPushDto dto)
    {
        var tenantId = _currentUserService.TenantId;
        if (tenantId == null)
            return BadRequest(new { message = "User is not a tenant" });

        var result = await _mpesaService.InitiateStkPushAsync(tenantId.Value, dto);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Query M-Pesa STK Push status for the current tenant
    /// </summary>
    /// <param name="checkoutRequestId">CheckoutRequestID from STK Push</param>
    /// <returns>STK Push query response</returns>
    [HttpGet("stk-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetStkStatus([FromQuery] string checkoutRequestId)
    {
        if (string.IsNullOrWhiteSpace(checkoutRequestId))
        {
            return BadRequest(new { message = "CheckoutRequestID is required" });
        }

        var result = await _mpesaService.QueryStkPushStatusAsync(checkoutRequestId);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Upload payment proof (screenshot, receipt, etc.)
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="file">Payment proof file</param>
    /// <returns>Updated payment with proof URL</returns>
    [HttpPost("{paymentId}/upload-proof")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadPaymentProof(int paymentId, IFormFile file)
    {
        var tenantId = _currentUserService.TenantId;
        if (tenantId == null)
            return BadRequest(new { message = "User is not a tenant" });

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file provided" });

        var result = await _paymentService.UploadPaymentProofAsync(paymentId, tenantId.Value, file);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}
