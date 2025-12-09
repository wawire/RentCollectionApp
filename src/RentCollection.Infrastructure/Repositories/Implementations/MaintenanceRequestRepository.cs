using Microsoft.EntityFrameworkCore;
using RentCollection.Application.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Repositories.Implementations
{
    public class MaintenanceRequestRepository : Repository<MaintenanceRequest>, IMaintenanceRequestRepository
    {
        public MaintenanceRequestRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<MaintenanceRequest>> GetByTenantIdAsync(int tenantId)
        {
            return await _context.MaintenanceRequests
                .Include(m => m.Tenant)
                .Include(m => m.Unit)
                .Include(m => m.Property)
                .Include(m => m.AssignedToUser)
                .Where(m => m.TenantId == tenantId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<MaintenanceRequest>> GetByPropertyIdAsync(int propertyId)
        {
            return await _context.MaintenanceRequests
                .Include(m => m.Tenant)
                .Include(m => m.Unit)
                .Include(m => m.Property)
                .Include(m => m.AssignedToUser)
                .Where(m => m.PropertyId == propertyId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<MaintenanceRequest>> GetByStatusAsync(MaintenanceRequestStatus status)
        {
            return await _context.MaintenanceRequests
                .Include(m => m.Tenant)
                .Include(m => m.Unit)
                .Include(m => m.Property)
                .Include(m => m.AssignedToUser)
                .Where(m => m.Status == status)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<MaintenanceRequest>> GetByAssignedUserIdAsync(int userId)
        {
            return await _context.MaintenanceRequests
                .Include(m => m.Tenant)
                .Include(m => m.Unit)
                .Include(m => m.Property)
                .Include(m => m.AssignedToUser)
                .Where(m => m.AssignedToUserId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<MaintenanceRequest?> GetWithDetailsAsync(int id)
        {
            return await _context.MaintenanceRequests
                .Include(m => m.Tenant)
                .Include(m => m.Unit)
                .Include(m => m.Property)
                .Include(m => m.AssignedToUser)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
