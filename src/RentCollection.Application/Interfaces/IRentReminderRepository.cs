using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Interfaces;

public interface IRentReminderRepository : IRepository<RentReminder>
{
    Task<IEnumerable<RentReminder>> GetRemindersByLandlordIdAsync(int landlordId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<RentReminder>> GetRemindersByTenantIdAsync(int tenantId);
    Task<IEnumerable<RentReminder>> GetScheduledRemindersAsync();
    Task<IEnumerable<RentReminder>> GetScheduledRemindersByTenantIdAsync(int tenantId, DateTime rentDueDate);
    Task<RentReminder?> GetReminderWithDetailsAsync(int id);
    Task<Dictionary<string, int>> GetRemindersByTypeAsync(int landlordId, DateTime? startDate = null, DateTime? endDate = null);
    Task<Dictionary<string, int>> GetRemindersByStatusAsync(int landlordId, DateTime? startDate = null, DateTime? endDate = null);
}
