using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.DTOs.RentReminders;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Enums;

namespace RentCollection.API.Controllers;

/// <summary>
/// Automated rent reminder management endpoints
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RentRemindersController : ControllerBase
{
    private readonly IRentReminderService _reminderService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RentRemindersController> _logger;

    public RentRemindersController(
        IRentReminderService reminderService,
        ICurrentUserService currentUserService,
        ILogger<RentRemindersController> logger)
    {
        _reminderService = reminderService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    #region Settings Management

    /// <summary>
    /// Get reminder settings for current landlord
    /// </summary>
    /// <returns>Reminder settings including enabled status, schedule, and message templates</returns>
    [HttpGet("settings")]
    [Authorize(Roles = "SystemAdmin,Landlord")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSettings()
    {
        try
        {
            var landlordId = int.Parse(_currentUserService.UserId!);
            var settings = await _reminderService.GetReminderSettingsAsync(landlordId);

            return Ok(new { success = true, data = settings });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reminder settings");
            return BadRequest(new { success = false, message = "Failed to retrieve reminder settings" });
        }
    }

    /// <summary>
    /// Update reminder settings for current landlord
    /// </summary>
    /// <param name="dto">Updated reminder settings</param>
    /// <returns>Updated settings</returns>
    [HttpPut("settings")]
    [Authorize(Roles = "SystemAdmin,Landlord")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateReminderSettingsDto dto)
    {
        try
        {
            var landlordId = int.Parse(_currentUserService.UserId!);
            var settings = await _reminderService.UpdateReminderSettingsAsync(landlordId, dto);

            return Ok(new { success = true, data = settings, message = "Reminder settings updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating reminder settings");
            return BadRequest(new { success = false, message = "Failed to update reminder settings" });
        }
    }

    #endregion

    #region Reminder History & Statistics

    /// <summary>
    /// Get reminder history for current landlord
    /// </summary>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>List of reminders</returns>
    [HttpGet("history")]
    [Authorize(Roles = "SystemAdmin,Landlord")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetHistory([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var landlordId = int.Parse(_currentUserService.UserId!);
            var reminders = await _reminderService.GetRemindersForLandlordAsync(landlordId, startDate, endDate);

            return Ok(new { success = true, data = reminders, count = reminders.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reminder history");
            return BadRequest(new { success = false, message = "Failed to retrieve reminder history" });
        }
    }

    /// <summary>
    /// Get reminder statistics for current landlord
    /// </summary>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>Reminder statistics including success rate and breakdowns</returns>
    [HttpGet("statistics")]
    [Authorize(Roles = "SystemAdmin,Landlord")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetStatistics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var landlordId = int.Parse(_currentUserService.UserId!);
            var statistics = await _reminderService.GetReminderStatisticsAsync(landlordId, startDate, endDate);

            return Ok(new { success = true, data = statistics });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reminder statistics");
            return BadRequest(new { success = false, message = "Failed to retrieve reminder statistics" });
        }
    }

    /// <summary>
    /// Get reminders for a specific tenant
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>List of reminders for the tenant</returns>
    [HttpGet("tenant/{tenantId}")]
    [Authorize(Roles = "SystemAdmin,Landlord,Tenant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTenantReminders(int tenantId)
    {
        try
        {
            // TODO: Add authorization check to ensure user can only view their own reminders if they're a tenant
            var reminders = await _reminderService.GetRemindersForTenantAsync(tenantId);

            return Ok(new { success = true, data = reminders, count = reminders.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant reminders for tenant {TenantId}", tenantId);
            return BadRequest(new { success = false, message = "Failed to retrieve tenant reminders" });
        }
    }

    #endregion

    #region Manual Reminder Operations

    /// <summary>
    /// Send a reminder immediately (manual trigger)
    /// </summary>
    /// <param name="id">Reminder ID</param>
    /// <returns>Success message</returns>
    [HttpPost("{id}/send")]
    [Authorize(Roles = "SystemAdmin,Landlord")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendReminder(int id)
    {
        try
        {
            await _reminderService.SendReminderNowAsync(id);
            return Ok(new { success = true, message = "Reminder sent successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Reminder {ReminderId} not found", id);
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending reminder {ReminderId}", id);
            return BadRequest(new { success = false, message = "Failed to send reminder" });
        }
    }

    /// <summary>
    /// Cancel a scheduled reminder
    /// </summary>
    /// <param name="id">Reminder ID</param>
    /// <returns>Success message</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SystemAdmin,Landlord")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelReminder(int id)
    {
        try
        {
            await _reminderService.CancelReminderAsync(id);
            return Ok(new { success = true, message = "Reminder cancelled successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Reminder {ReminderId} not found", id);
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling reminder {ReminderId}", id);
            return BadRequest(new { success = false, message = "Failed to cancel reminder" });
        }
    }

    /// <summary>
    /// Schedule reminders for a specific tenant (manual trigger)
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Success message</returns>
    [HttpPost("schedule/tenant/{tenantId}")]
    [Authorize(Roles = "SystemAdmin,Landlord")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ScheduleTenantReminders(int tenantId)
    {
        try
        {
            await _reminderService.ScheduleRemindersForTenantAsync(tenantId);
            return Ok(new { success = true, message = "Reminders scheduled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling reminders for tenant {TenantId}", tenantId);
            return BadRequest(new { success = false, message = "Failed to schedule reminders" });
        }
    }

    /// <summary>
    /// Schedule reminders for all tenants (manual trigger - SystemAdmin only)
    /// </summary>
    /// <returns>Success message</returns>
    [HttpPost("schedule/all")]
    [Authorize(Roles = "SystemAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ScheduleAllReminders()
    {
        try
        {
            await _reminderService.ScheduleRemindersForAllTenantsAsync();
            return Ok(new { success = true, message = "Reminders scheduled for all tenants successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling reminders for all tenants");
            return BadRequest(new { success = false, message = "Failed to schedule reminders" });
        }
    }

    #endregion

    #region Tenant Preferences

    /// <summary>
    /// Update reminder preferences for a tenant
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="remindersEnabled">Enable or disable reminders</param>
    /// <param name="preferredChannel">Preferred notification channel (SMS, Email, Both)</param>
    /// <returns>Success message</returns>
    [HttpPut("preferences/tenant/{tenantId}")]
    [Authorize(Roles = "SystemAdmin,Landlord,Tenant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTenantPreferences(
        int tenantId,
        [FromQuery] bool remindersEnabled,
        [FromQuery] ReminderChannel preferredChannel)
    {
        try
        {
            // TODO: Add authorization check to ensure user can only update their own preferences if they're a tenant
            await _reminderService.UpdateTenantPreferencesAsync(tenantId, remindersEnabled, preferredChannel);
            return Ok(new { success = true, message = "Tenant preferences updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant preferences for tenant {TenantId}", tenantId);
            return BadRequest(new { success = false, message = "Failed to update tenant preferences" });
        }
    }

    #endregion
}
