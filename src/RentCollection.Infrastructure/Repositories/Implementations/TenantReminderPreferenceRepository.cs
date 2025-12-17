using Microsoft.EntityFrameworkCore;
using RentCollection.Application.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Repositories.Implementations;

public class TenantReminderPreferenceRepository : Repository<TenantReminderPreference>, ITenantReminderPreferenceRepository
{
    public TenantReminderPreferenceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<TenantReminderPreference?> GetByTenantIdAsync(int tenantId)
    {
        return await _context.TenantReminderPreferences
            .FirstOrDefaultAsync(p => p.TenantId == tenantId);
    }
}
