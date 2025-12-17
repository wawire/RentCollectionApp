using RentCollection.Application.DTOs.RentReminders;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Interfaces
{
    public interface IRentReminderService
    {
        // Settings management
        Task<ReminderSettingsDto> GetReminderSettingsAsync(int landlordId);
        Task<ReminderSettingsDto> UpdateReminderSettingsAsync(int landlordId, UpdateReminderSettingsDto dto);
        Task<ReminderSettingsDto> GetOrCreateDefaultSettingsAsync(int landlordId);

        // Reminder scheduling (called by background service)
        Task ScheduleRemindersForAllTenantsAsync();
        Task ScheduleRemindersForTenantAsync(int tenantId);

        // Manual reminder operations
        Task SendReminderNowAsync(int reminderId);
        Task CancelReminderAsync(int reminderId);

        // Query reminders
        Task<List<RentReminderDto>> GetRemindersForLandlordAsync(int landlordId, DateTime? startDate = null, DateTime? endDate = null);
        Task<List<RentReminderDto>> GetRemindersForTenantAsync(int tenantId);
        Task<ReminderStatisticsDto> GetReminderStatisticsAsync(int landlordId, DateTime? startDate = null, DateTime? endDate = null);

        // Tenant preferences
        Task<bool> UpdateTenantPreferencesAsync(int tenantId, bool remindersEnabled, ReminderChannel preferredChannel);
    }
}
