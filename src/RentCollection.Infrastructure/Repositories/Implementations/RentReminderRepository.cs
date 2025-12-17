using Microsoft.EntityFrameworkCore;
using RentCollection.Application.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Repositories.Implementations;

public class RentReminderRepository : Repository<RentReminder>, IRentReminderRepository
{
    public RentReminderRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<RentReminder>> GetRemindersByLandlordIdAsync(int landlordId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.RentReminders
            .Include(r => r.Tenant)
            .Include(r => r.Property)
            .Include(r => r.Unit)
            .Where(r => r.LandlordId == landlordId);

        if (startDate.HasValue)
        {
            query = query.Where(r => r.ScheduledDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(r => r.ScheduledDate <= endDate.Value);
        }

        return await query
            .OrderByDescending(r => r.ScheduledDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<RentReminder>> GetRemindersByTenantIdAsync(int tenantId)
    {
        return await _context.RentReminders
            .Include(r => r.Tenant)
            .Include(r => r.Property)
            .Include(r => r.Unit)
            .Where(r => r.TenantId == tenantId)
            .OrderByDescending(r => r.ScheduledDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<RentReminder>> GetScheduledRemindersAsync()
    {
        var now = DateTime.UtcNow;

        return await _context.RentReminders
            .Include(r => r.Tenant)
            .Include(r => r.Property)
            .Include(r => r.Unit)
            .Include(r => r.Landlord)
            .Where(r => r.Status == ReminderStatus.Scheduled &&
                       r.ScheduledDate <= now)
            .OrderBy(r => r.ScheduledDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<RentReminder>> GetScheduledRemindersByTenantIdAsync(int tenantId, DateTime rentDueDate)
    {
        return await _context.RentReminders
            .Where(r => r.TenantId == tenantId &&
                       r.RentDueDate == rentDueDate &&
                       r.Status == ReminderStatus.Scheduled)
            .ToListAsync();
    }

    public async Task<RentReminder?> GetReminderWithDetailsAsync(int id)
    {
        return await _context.RentReminders
            .Include(r => r.Tenant)
            .Include(r => r.Landlord)
            .Include(r => r.Property)
            .Include(r => r.Unit)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Dictionary<string, int>> GetRemindersByTypeAsync(int landlordId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.RentReminders
            .Where(r => r.LandlordId == landlordId);

        if (startDate.HasValue)
        {
            query = query.Where(r => r.ScheduledDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(r => r.ScheduledDate <= endDate.Value);
        }

        return await query
            .GroupBy(r => r.ReminderType)
            .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count);
    }

    public async Task<Dictionary<string, int>> GetRemindersByStatusAsync(int landlordId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.RentReminders
            .Where(r => r.LandlordId == landlordId);

        if (startDate.HasValue)
        {
            query = query.Where(r => r.ScheduledDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(r => r.ScheduledDate <= endDate.Value);
        }

        return await query
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);
    }
}
