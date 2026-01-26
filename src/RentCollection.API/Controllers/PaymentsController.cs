using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.Authorization;
using RentCollection.Application.DTOs.Payments;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.API.Controllers;

/// <summary>
/// Payments management endpoints
/// </summary>
[Authorize]
[Authorize(Policy = Policies.RequireVerifiedUser)]
[Authorize(Policy = Policies.RequireActiveOrganization)]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IMPesaService _mpesaService;
    private readonly ILogger<PaymentsController> _logger;
    private readonly ICurrentUserService _currentUserService;

    public PaymentsController(
        IPaymentService paymentService,
        IMPesaService mpesaService,
        ILogger<PaymentsController> logger,
        ICurrentUserService currentUserService)
    {
        _paymentService = paymentService;
        _mpesaService = mpesaService;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get all payments
    /// </summary>
    /// <returns>List of all payments</returns>
    [HttpGet]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager,Accountant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _paymentService.GetAllPaymentsAsync();

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get paginated payments
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <returns>Paginated list of payments</returns>
    [HttpGet("paginated")]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager,Accountant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _paymentService.GetPaymentsPaginatedAsync(pageNumber, pageSize);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get payment by ID
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <returns>Payment details</returns>
    [HttpGet("{id}")]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager,Accountant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _paymentService.GetPaymentByIdAsync(id);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get payments by tenant ID
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>List of payments for the specified tenant</returns>
    [HttpGet("tenant/{tenantId}")]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager,Accountant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByTenantId(int tenantId)
    {
        var result = await _paymentService.GetPaymentsByTenantIdAsync(tenantId);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Record a new payment
    /// </summary>
    /// <param name="createDto">Payment creation data</param>
    /// <returns>Created payment</returns>
    [HttpPost]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePaymentDto createDto)
    {
        var result = await _paymentService.CreatePaymentAsync(createDto);

        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Delete a payment
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "PlatformAdmin,Landlord")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _paymentService.DeletePaymentAsync(id);

        if (!result.IsSuccess)
            return BadRequest(result);

        return NoContent();
    }

    /// <summary>
    /// Get pending payments awaiting confirmation (Landlord/Caretaker only)
    /// </summary>
    /// <param name="propertyId">Optional property filter</param>
    /// <returns>List of pending payments</returns>
    [HttpGet("pending")]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingPayments([FromQuery] int? propertyId = null)
    {
        var result = await _paymentService.GetPendingPaymentsAsync(propertyId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Confirm a payment (Landlord/Caretaker only)
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <returns>Updated payment</returns>
    [HttpPut("{id}/confirm")]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmPayment(int id)
    {
        var confirmedByUserId = _currentUserService.UserIdInt;
        if (!confirmedByUserId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _paymentService.ConfirmPaymentAsync(id, confirmedByUserId.Value);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Reject a payment (Landlord/Caretaker only)
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <param name="request">Rejection request with reason</param>
    /// <returns>Updated payment</returns>
    [HttpPut("{id}/reject")]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectPayment(int id, [FromBody] RejectPaymentRequest request)
    {
        var result = await _paymentService.RejectPaymentAsync(id, request.Reason);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get overdue payments (pending payments past due date)
    /// </summary>
    /// <param name="propertyId">Optional property ID filter (for landlords)</param>
    /// <returns>List of overdue payments</returns>
    [HttpGet("overdue")]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager,Accountant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOverduePayments([FromQuery] int? propertyId = null)
    {
        var result = await _paymentService.GetOverduePaymentsAsync(propertyId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Query M-Pesa STK Push status
    /// </summary>
    /// <param name="checkoutRequestId">CheckoutRequestID from STK Push</param>
    /// <returns>STK Push query response</returns>
    [HttpGet("stk-status")]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager,Accountant")]
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
    /// Calculate late fee for a payment (preview without applying)
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <returns>Late fee calculation details</returns>
    [HttpGet("{id}/calculate-late-fee")]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager,Accountant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CalculateLateFee(int id)
    {
        var result = await _paymentService.CalculateLateFeeAsync(id);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Apply late fee to an overdue payment
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <returns>Updated payment with late fee</returns>
    [HttpPost("{id}/apply-late-fee")]
    [Authorize(Roles = "PlatformAdmin,Landlord")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApplyLateFee(int id)
    {
        var result = await _paymentService.ApplyLateFeeAsync(id);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}

/// <summary>
/// Request model for rejecting a payment
/// </summary>
public class RejectPaymentRequest
{
    public string Reason { get; set; } = string.Empty;
}

