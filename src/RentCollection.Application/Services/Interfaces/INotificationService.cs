using RentCollection.Application.Common;

namespace RentCollection.Application.Services.Interfaces
{
    public interface INotificationService
    {
        Task<ServiceResult<bool>> SendPaymentReminderToTenantAsync(int tenantId, int daysBeforeDue = 3);
        Task<ServiceResult<int>> SendUpcomingPaymentRemindersAsync(int daysUntilDue = 3, int? landlordId = null);
        Task<ServiceResult<int>> SendOverdueNoticesAsync(int? landlordId = null);
        Task<ServiceResult<bool>> SendOverdueNoticeToTenantAsync(int tenantId);
        Task<ServiceResult<bool>> SendPaymentReceiptAsync(int paymentId);
    }
}
