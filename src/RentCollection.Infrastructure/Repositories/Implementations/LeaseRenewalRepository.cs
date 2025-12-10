using Microsoft.EntityFrameworkCore;
using RentCollection.Application.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Repositories.Implementations
{
    public class LeaseRenewalRepository : Repository<LeaseRenewal>, ILeaseRenewalRepository
    {
        public LeaseRenewalRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<LeaseRenewal>> GetByTenantIdAsync(int tenantId)
        {
            return await _context.LeaseRenewals
                .Include(lr => lr.Tenant)
                .Include(lr => lr.Unit)
                .Include(lr => lr.Property)
                .Where(lr => lr.TenantId == tenantId)
                .OrderByDescending(lr => lr.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<LeaseRenewal>> GetByPropertyIdAsync(int propertyId)
        {
            return await _context.LeaseRenewals
                .Include(lr => lr.Tenant)
                .Include(lr => lr.Unit)
                .Include(lr => lr.Property)
                .Where(lr => lr.PropertyId == propertyId)
                .OrderByDescending(lr => lr.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<LeaseRenewal>> GetByStatusAsync(LeaseRenewalStatus status)
        {
            return await _context.LeaseRenewals
                .Include(lr => lr.Tenant)
                .Include(lr => lr.Unit)
                .Include(lr => lr.Property)
                .Where(lr => lr.Status == status)
                .OrderByDescending(lr => lr.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<LeaseRenewal>> GetExpiringSoonAsync(int daysUntilExpiry = 60)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(daysUntilExpiry);
            return await _context.LeaseRenewals
                .Include(lr => lr.Tenant)
                .Include(lr => lr.Unit)
                .Include(lr => lr.Property)
                .Where(lr => lr.CurrentLeaseEndDate <= cutoffDate
                    && lr.CurrentLeaseEndDate >= DateTime.UtcNow
                    && lr.Status == LeaseRenewalStatus.Pending)
                .OrderBy(lr => lr.CurrentLeaseEndDate)
                .ToListAsync();
        }

        public async Task<LeaseRenewal?> GetWithDetailsAsync(int id)
        {
            return await _context.LeaseRenewals
                .Include(lr => lr.Tenant)
                .Include(lr => lr.Unit)
                .Include(lr => lr.Property)
                .FirstOrDefaultAsync(lr => lr.Id == id);
        }

        public async Task<LeaseRenewal?> GetActiveRenewalForTenantAsync(int tenantId)
        {
            return await _context.LeaseRenewals
                .Include(lr => lr.Tenant)
                .Include(lr => lr.Unit)
                .Include(lr => lr.Property)
                .Where(lr => lr.TenantId == tenantId
                    && (lr.Status == LeaseRenewalStatus.Pending
                        || lr.Status == LeaseRenewalStatus.TenantAccepted))
                .OrderByDescending(lr => lr.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}
