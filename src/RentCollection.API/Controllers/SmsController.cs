using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.DTOs.Sms;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.API.Controllers;

/// <summary>
/// SMS management endpoints
/// </summary>
[Authorize(Roles = "PlatformAdmin,Landlord,Manager,Accountant,Caretaker")]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SmsController : ControllerBase
{
    private readonly ISmsService _smsService;
    private readonly ILogger<SmsController> _logger;

    public SmsController(ISmsService smsService, ILogger<SmsController> logger)
    {
        _smsService = smsService;
        _logger = logger;
    }

    /// <summary>
    /// Send a custom SMS message
    /// </summary>
    /// <param name="sendSmsDto">SMS details including phone number and message</param>
    /// <returns>Success or failure result</returns>
    [HttpPost("send")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendSms([FromBody] SendSmsDto sendSmsDto)
    {
        var result = await _smsService.SendSmsAsync(sendSmsDto);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Send rent reminder SMS to a tenant
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Success or failure result</returns>
    [HttpPost("rent-reminder/{tenantId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendRentReminder(int tenantId)
    {
        var result = await _smsService.SendRentReminderAsync(tenantId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Send payment receipt SMS for a specific payment
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <returns>Success or failure result</returns>
    [HttpPost("payment-receipt/{paymentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendPaymentReceipt(int paymentId)
    {
        var result = await _smsService.SendPaymentReceiptAsync(paymentId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}

