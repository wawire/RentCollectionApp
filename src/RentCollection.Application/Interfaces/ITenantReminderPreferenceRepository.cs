using RentCollection.Domain.Entities;

namespace RentCollection.Application.Interfaces;

public interface ITenantReminderPreferenceRepository : IRepository<TenantReminderPreference>
{
    Task<TenantReminderPreference?> GetByTenantIdAsync(int tenantId);
}
