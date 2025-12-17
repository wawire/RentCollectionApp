using Microsoft.EntityFrameworkCore;
using RentCollection.Application.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Repositories.Implementations;

public class ReminderSettingsRepository : Repository<ReminderSettings>, IReminderSettingsRepository
{
    public ReminderSettingsRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ReminderSettings?> GetByLandlordIdAsync(int landlordId)
    {
        return await _context.ReminderSettings
            .FirstOrDefaultAsync(s => s.LandlordId == landlordId);
    }
}
