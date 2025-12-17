using RentCollection.Domain.Entities;

namespace RentCollection.Application.Interfaces;

public interface IReminderSettingsRepository : IRepository<ReminderSettings>
{
    Task<ReminderSettings?> GetByLandlordIdAsync(int landlordId);
}
