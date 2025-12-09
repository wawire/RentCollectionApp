using RentCollection.Application.Common.Models;

namespace RentCollection.Application.Services.Interfaces;

public interface INotificationService
{
    /// <summary>
    /// Send payment reminder to a specific tenant (both email and SMS)
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="daysBeforeDue">Days before due date for the reminder</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> SendPaymentReminderToTenantAsync(int tenantId, int daysBeforeDue = 3);

    /// <summary>
    /// Send payment reminders to all tenants with upcoming due dates
    /// </summary>
    /// <param name="daysBeforeDue">Days before due date to trigger reminder</param>
    /// <param name="landlordId">Optional: Filter by landlord ID</param>
    /// <returns>Result with count of reminders sent</returns>
    Task<Result<int>> SendUpcomingPaymentRemindersAsync(int daysBeforeDue = 3, int? landlordId = null);

    /// <summary>
    /// Send overdue payment notice to a specific tenant
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> SendOverdueNoticeToTenantAsync(int tenantId);

    /// <summary>
    /// Send overdue notices to all tenants with overdue payments
    /// </summary>
    /// <param name="landlordId">Optional: Filter by landlord ID</param>
    /// <returns>Result with count of notices sent</returns>
    Task<Result<int>> SendOverdueNoticesAsync(int? landlordId = null);

    /// <summary>
    /// Send payment receipt notification to tenant (both email and SMS)
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> SendPaymentReceiptAsync(int paymentId);
}
