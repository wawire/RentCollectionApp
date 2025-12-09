using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.DTOs.Payments;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.API.Controllers;

/// <summary>
/// Payments management endpoints
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Get all payments
    /// </summary>
    /// <returns>List of all payments</returns>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin,Landlord,Caretaker,Accountant,Tenant")]
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
    [Authorize(Roles = "SystemAdmin,Landlord,Caretaker,Accountant,Tenant")]
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
    [Authorize(Roles = "SystemAdmin,Landlord,Caretaker,Accountant,Tenant")]
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
    [Authorize(Roles = "SystemAdmin,Landlord,Caretaker,Accountant,Tenant")]
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
    [Authorize(Roles = "SystemAdmin,Landlord,Caretaker")]
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
    [Authorize(Roles = "SystemAdmin,Landlord")]
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
    [Authorize(Roles = "SystemAdmin,Landlord,Caretaker")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingPayments([FromQuery] int? propertyId = null)
    {
        // Get current user ID (would come from authentication context)
        // For now, assuming landlordId is passed or derived from auth
        var landlordId = 1; // TODO: Get from authenticated user context

        var result = await _paymentService.GetPendingPaymentsAsync(landlordId, propertyId);

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
    [Authorize(Roles = "SystemAdmin,Landlord,Caretaker")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmPayment(int id)
    {
        // Get current user ID from authentication context
        var confirmedByUserId = 1; // TODO: Get from authenticated user

        var result = await _paymentService.ConfirmPaymentAsync(id, confirmedByUserId);

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
    [Authorize(Roles = "SystemAdmin,Landlord,Caretaker")]
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
    [Authorize(Roles = "SystemAdmin,Landlord,Accountant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOverduePayments([FromQuery] int? propertyId = null)
    {
        var result = await _paymentService.GetOverduePaymentsAsync(propertyId);

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
