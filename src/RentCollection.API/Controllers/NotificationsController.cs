using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.API.Controllers;

/// <summary>
/// Payment reminder and notification endpoints
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Send payment reminder to a specific tenant
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="daysBeforeDue">Days before due date (default: 3)</param>
    /// <returns>Success message</returns>
    [HttpPost("payment-reminder/tenant/{tenantId}")]
    [Authorize(Roles = "SystemAdmin,Landlord,Accountant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendPaymentReminderToTenant(int tenantId, [FromQuery] int daysBeforeDue = 3)
    {
        var result = await _notificationService.SendPaymentReminderToTenantAsync(tenantId, daysBeforeDue);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Send payment reminders to all tenants with upcoming due dates
    /// </summary>
    /// <param name="daysBeforeDue">Days before due date to trigger reminder (default: 3)</param>
    /// <param name="landlordId">Optional: Filter by landlord ID</param>
    /// <returns>Count of reminders sent</returns>
    [HttpPost("payment-reminder/bulk")]
    [Authorize(Roles = "SystemAdmin,Landlord,Accountant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendUpcomingPaymentReminders(
        [FromQuery] int daysBeforeDue = 3,
        [FromQuery] int? landlordId = null)
    {
        var result = await _notificationService.SendUpcomingPaymentRemindersAsync(daysBeforeDue, landlordId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Send overdue payment notice to a specific tenant
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Success message</returns>
    [HttpPost("overdue-notice/tenant/{tenantId}")]
    [Authorize(Roles = "SystemAdmin,Landlord,Accountant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendOverdueNoticeToTenant(int tenantId)
    {
        var result = await _notificationService.SendOverdueNoticeToTenantAsync(tenantId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Send overdue notices to all tenants with overdue payments
    /// </summary>
    /// <param name="landlordId">Optional: Filter by landlord ID</param>
    /// <returns>Count of notices sent</returns>
    [HttpPost("overdue-notice/bulk")]
    [Authorize(Roles = "SystemAdmin,Landlord,Accountant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendOverdueNotices([FromQuery] int? landlordId = null)
    {
        var result = await _notificationService.SendOverdueNoticesAsync(landlordId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Send payment receipt notification to tenant
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <returns>Success message</returns>
    [HttpPost("payment-receipt/{paymentId}")]
    [Authorize(Roles = "SystemAdmin,Landlord,Accountant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendPaymentReceipt(int paymentId)
    {
        var result = await _notificationService.SendPaymentReceiptAsync(paymentId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}
