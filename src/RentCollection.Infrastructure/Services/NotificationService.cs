using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Common;
using RentCollection.Application.Common.Models;
using RentCollection.Application.Helpers;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        ApplicationDbContext context,
        IEmailService emailService,
        ISmsService smsService,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _emailService = emailService;
        _smsService = smsService;
        _logger = logger;
    }

    public async Task<ServiceResult<bool>> SendPaymentReminderToTenantAsync(int tenantId)
    {
        try
        {
            var tenant = await _context.Tenants
                .Include(t => t.Unit)
                    .ThenInclude(u => u.Property)
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
            {
                return ServiceResult<bool>.Failure($"Tenant with ID {tenantId} not found");
            }

            // Calculate the next due date
            var nextDueDate = PaymentDueDateHelper.CalculateNextMonthDueDate(tenant.RentDueDay);

            var propertyName = tenant.Unit?.Property?.Name ?? "Your Property";
            var unitNumber = tenant.Unit?.UnitNumber ?? "N/A";

            // Send email reminder
            if (!string.IsNullOrWhiteSpace(tenant.Email))
            {
                try
                {
                    await _emailService.SendRentReminderEmailAsync(
                        tenant.Email,
                        tenant.FullName,
                        propertyName,
                        unitNumber,
                        tenant.MonthlyRent,
                        nextDueDate);

                    _logger.LogInformation("Payment reminder email sent to tenant {TenantId} ({TenantName})",
                        tenantId, tenant.FullName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send payment reminder email to tenant {TenantId}", tenantId);
                }
            }

            // Send SMS reminder
            if (!string.IsNullOrWhiteSpace(tenant.PhoneNumber))
            {
                try
                {
                    await _smsService.SendRentReminderAsync(tenantId);
                    _logger.LogInformation("Payment reminder SMS sent to tenant {TenantId} ({TenantName})",
                        tenantId, tenant.FullName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send payment reminder SMS to tenant {TenantId}", tenantId);
                }
            }

            return ServiceResult<bool>.Success(true, "Payment reminder sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending payment reminder to tenant {TenantId}", tenantId);
            return ServiceResult<bool>.Failure($"Error sending payment reminder: {ex.Message}");
        }
    }

    public async Task<ServiceResult<int>> SendUpcomingPaymentRemindersAsync(int daysUntilDue = 3)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var targetDate = today.AddDays(daysUntilDue);

            // Get all active tenants
            var tenantsQuery = _context.Tenants
                .Include(t => t.Unit)
                    .ThenInclude(u => u.Property)
                .Where(t => t.IsActive);

            var tenants = await tenantsQuery.ToListAsync();
            int remindersSent = 0;

            foreach (var tenant in tenants)
            {
                try
                {
                    // Calculate next due date for this tenant
                    var nextDueDate = PaymentDueDateHelper.CalculateNextMonthDueDate(tenant.RentDueDay);

                    // Check if reminder should be sent (due date is within the target window)
                    if (nextDueDate.Date == targetDate.Date)
                    {
                        var result = await SendPaymentReminderToTenantAsync(tenant.Id);
                        if (result.IsSuccess)
                        {
                            remindersSent++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send reminder to tenant {TenantId}", tenant.Id);
                    // Continue with next tenant
                }
            }

            _logger.LogInformation("Sent {Count} payment reminders for payments due in {Days} days",
                remindersSent, daysUntilDue);

            return ServiceResult<int>.Success(remindersSent, $"Sent {remindersSent} payment reminder(s)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending upcoming payment reminders");
            return ServiceResult<int>.Failure($"Error sending reminders: {ex.Message}");
        }
    }

    public async Task<ServiceResult<int>> SendOverdueNoticesAsync()
    {
        try
        {
            var today = DateTime.UtcNow.Date;

            // Get all tenants with overdue payments
            var tenantsQuery = _context.Tenants
                .Include(t => t.Unit)
                    .ThenInclude(u => u.Property)
                .Include(t => t.Payments)
                .Where(t => t.IsActive && t.Payments.Any(p =>
                    p.Status == Domain.Enums.PaymentStatus.Pending && p.DueDate.Date < today));

            var tenants = await tenantsQuery.ToListAsync();
            int noticesSent = 0;

            foreach (var tenant in tenants)
            {
                try
                {
                    var result = await SendOverdueNoticeToTenantAsync(tenant.Id);
                    if (result.IsSuccess)
                    {
                        noticesSent++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send overdue notice to tenant {TenantId}", tenant.Id);
                    // Continue with next tenant
                }
            }

            _logger.LogInformation("Sent {Count} overdue payment notices", noticesSent);

            return ServiceResult<int>.Success(noticesSent, $"Sent {noticesSent} overdue notice(s)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending overdue notices");
            return ServiceResult<int>.Failure($"Error sending overdue notices: {ex.Message}");
        }
    }

    // Helper method not in interface - used internally
    private async Task<Result> SendOverdueNoticeToTenantAsync(int tenantId)
    {
        try
        {
            var tenant = await _context.Tenants
                .Include(t => t.Unit)
                    .ThenInclude(u => u.Property)
                .Include(t => t.Payments)
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
            {
                return Result.Failure($"Tenant with ID {tenantId} not found");
            }

            // Get overdue payments for this tenant
            var today = DateTime.UtcNow.Date;
            var overduePayments = tenant.Payments
                .Where(p => p.Status == Domain.Enums.PaymentStatus.Pending && p.DueDate.Date < today)
                .ToList();

            if (!overduePayments.Any())
            {
                return Result.Failure("No overdue payments found for this tenant");
            }

            var totalOverdue = overduePayments.Sum(p => p.Amount + p.LateFeeAmount);
            var oldestOverduePayment = overduePayments.OrderBy(p => p.DueDate).First();
            var daysOverdue = (today - oldestOverduePayment.DueDate.Date).Days;

            var propertyName = tenant.Unit?.Property?.Name ?? "Your Property";
            var unitNumber = tenant.Unit?.UnitNumber ?? "N/A";

            // Send email notice
            if (!string.IsNullOrWhiteSpace(tenant.Email))
            {
                try
                {
                    await _emailService.SendOverdueNoticeEmailAsync(
                        tenant.Email,
                        tenant.FullName,
                        propertyName,
                        unitNumber,
                        totalOverdue,
                        daysOverdue);

                    _logger.LogInformation("Overdue notice email sent to tenant {TenantId} ({TenantName})",
                        tenantId, tenant.FullName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send overdue notice email to tenant {TenantId}", tenantId);
                }
            }

            // Send SMS notice
            if (!string.IsNullOrWhiteSpace(tenant.PhoneNumber))
            {
                try
                {
                    var smsMessage = SmsTemplates.GetOverdueNoticeMessage(
                        tenant.FullName,
                        totalOverdue,
                        daysOverdue,
                        propertyName,
                        unitNumber);

                    await _smsService.SendSmsAsync(new Application.DTOs.Sms.SendSmsDto
                    {
                        PhoneNumber = tenant.PhoneNumber,
                        Message = smsMessage,
                        TenantId = tenantId
                    });

                    _logger.LogInformation("Overdue notice SMS sent to tenant {TenantId} ({TenantName})",
                        tenantId, tenant.FullName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send overdue notice SMS to tenant {TenantId}", tenantId);
                }
            }

            return Result.Success("Overdue notice sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending overdue notice to tenant {TenantId}", tenantId);
            return Result.Failure($"Error sending overdue notice: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> SendPaymentReceiptAsync(int paymentId)
    {
        try
        {
            var payment = await _context.Payments
                .Include(p => p.Tenant)
                    .ThenInclude(t => t.Unit)
                        .ThenInclude(u => u.Property)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return ServiceResult<bool>.Failure($"Payment with ID {paymentId} not found");
            }

            var tenant = payment.Tenant;
            var propertyName = tenant.Unit?.Property?.Name ?? "Your Property";
            var unitNumber = tenant.Unit?.UnitNumber ?? "N/A";
            var referenceNumber = payment.TransactionReference ?? $"RCP-{paymentId:D6}";

            // Send email receipt
            if (!string.IsNullOrWhiteSpace(tenant.Email))
            {
                try
                {
                    await _emailService.SendPaymentReceiptEmailAsync(
                        tenant.Email,
                        tenant.FullName,
                        propertyName,
                        unitNumber,
                        payment.Amount,
                        payment.PaymentDate,
                        referenceNumber);

                    _logger.LogInformation("Payment receipt email sent for payment {PaymentId}", paymentId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send payment receipt email for payment {PaymentId}", paymentId);
                }
            }

            // Send SMS receipt
            if (!string.IsNullOrWhiteSpace(tenant.PhoneNumber))
            {
                try
                {
                    await _smsService.SendPaymentReceiptAsync(paymentId);
                    _logger.LogInformation("Payment receipt SMS sent for payment {PaymentId}", paymentId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send payment receipt SMS for payment {PaymentId}", paymentId);
                }
            }

            return ServiceResult<bool>.Success(true, "Payment receipt sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending payment receipt for payment {PaymentId}", paymentId);
            return ServiceResult<bool>.Failure($"Error sending payment receipt: {ex.Message}");
        }
    }
}
