using RentCollection.Application.Common;

namespace RentCollection.Application.Services.Interfaces
{
    public interface INotificationService
    {
        Task<ServiceResult<bool>> SendPaymentReminderToTenantAsync(int tenantId);
        Task<ServiceResult<int>> SendUpcomingPaymentRemindersAsync(int daysUntilDue = 3);
        Task<ServiceResult<int>> SendOverdueNoticesAsync();
        Task<ServiceResult<bool>> SendPaymentReceiptAsync(int paymentId);
    }
}
