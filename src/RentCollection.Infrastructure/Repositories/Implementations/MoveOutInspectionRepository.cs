using Microsoft.EntityFrameworkCore;
using RentCollection.Application.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Repositories.Implementations;

public class MoveOutInspectionRepository : Repository<MoveOutInspection>, IMoveOutInspectionRepository
{
    public MoveOutInspectionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<MoveOutInspection?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.MoveOutInspections
            .Include(m => m.Tenant)
            .Include(m => m.Unit)
            .Include(m => m.Property)
            .Include(m => m.InspectedBy)
            .Include(m => m.InspectionItems)
                .ThenInclude(i => i.Photos)
            .Include(m => m.Photos)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<List<MoveOutInspection>> GetByTenantIdAsync(int tenantId)
    {
        return await _context.MoveOutInspections
            .Include(m => m.Tenant)
            .Include(m => m.Unit)
            .Include(m => m.Property)
            .Include(m => m.InspectedBy)
            .Include(m => m.InspectionItems)
            .Include(m => m.Photos)
            .Where(m => m.TenantId == tenantId)
            .OrderByDescending(m => m.InspectionDate)
            .ToListAsync();
    }

    public async Task<List<MoveOutInspection>> GetByUnitIdAsync(int unitId)
    {
        return await _context.MoveOutInspections
            .Include(m => m.Tenant)
            .Include(m => m.Unit)
            .Include(m => m.Property)
            .Include(m => m.InspectedBy)
            .Include(m => m.InspectionItems)
            .Include(m => m.Photos)
            .Where(m => m.UnitId == unitId)
            .OrderByDescending(m => m.InspectionDate)
            .ToListAsync();
    }

    public async Task<List<MoveOutInspection>> GetByPropertyIdAsync(int propertyId)
    {
        return await _context.MoveOutInspections
            .Include(m => m.Tenant)
            .Include(m => m.Unit)
            .Include(m => m.Property)
            .Include(m => m.InspectedBy)
            .Include(m => m.InspectionItems)
            .Include(m => m.Photos)
            .Where(m => m.PropertyId == propertyId)
            .OrderByDescending(m => m.InspectionDate)
            .ToListAsync();
    }

    public async Task<List<MoveOutInspection>> GetByPropertyIdAsync(int propertyId, int landlordUserId)
    {
        return await _context.MoveOutInspections
            .Include(m => m.Tenant)
            .Include(m => m.Unit)
            .Include(m => m.Property)
            .Include(m => m.InspectedBy)
            .Include(m => m.InspectionItems)
            .Include(m => m.Photos)
            .Where(m => m.PropertyId == propertyId && m.Property.LandlordId == landlordUserId)
            .OrderByDescending(m => m.InspectionDate)
            .ToListAsync();
    }

    public async Task<List<MoveOutInspection>> GetByStatusAsync(MoveOutInspectionStatus status)
    {
        return await _context.MoveOutInspections
            .Include(m => m.Tenant)
            .Include(m => m.Unit)
            .Include(m => m.Property)
            .Include(m => m.InspectedBy)
            .Where(m => m.Status == status)
            .OrderByDescending(m => m.InspectionDate)
            .ToListAsync();
    }

    public async Task<List<MoveOutInspection>> GetByStatusAsync(MoveOutInspectionStatus status, int landlordUserId)
    {
        return await _context.MoveOutInspections
            .Include(m => m.Tenant)
            .Include(m => m.Unit)
            .Include(m => m.Property)
            .Include(m => m.InspectedBy)
            .Where(m => m.Status == status && m.Property.LandlordId == landlordUserId)
            .OrderByDescending(m => m.InspectionDate)
            .ToListAsync();
    }

    public async Task<List<MoveOutInspection>> GetPendingInspectionsAsync()
    {
        return await _context.MoveOutInspections
            .Include(m => m.Tenant)
            .Include(m => m.Unit)
            .Include(m => m.Property)
            .Include(m => m.InspectedBy)
            .Where(m => m.Status == MoveOutInspectionStatus.Scheduled ||
                        m.Status == MoveOutInspectionStatus.InProgress)
            .OrderBy(m => m.InspectionDate)
            .ToListAsync();
    }

    public async Task<List<MoveOutInspection>> GetPendingInspectionsAsync(int landlordUserId)
    {
        return await _context.MoveOutInspections
            .Include(m => m.Tenant)
            .Include(m => m.Unit)
            .Include(m => m.Property)
            .Include(m => m.InspectedBy)
            .Where(m => (m.Status == MoveOutInspectionStatus.Scheduled ||
                        m.Status == MoveOutInspectionStatus.InProgress) &&
                       m.Property.LandlordId == landlordUserId)
            .OrderBy(m => m.InspectionDate)
            .ToListAsync();
    }

    public async Task<MoveOutInspection?> GetLatestByTenantIdAsync(int tenantId)
    {
        return await _context.MoveOutInspections
            .Include(m => m.Tenant)
            .Include(m => m.Unit)
            .Include(m => m.Property)
            .Include(m => m.InspectedBy)
            .Include(m => m.InspectionItems)
                .ThenInclude(i => i.Photos)
            .Include(m => m.Photos)
            .Where(m => m.TenantId == tenantId)
            .OrderByDescending(m => m.InspectionDate)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> HasPendingInspectionAsync(int tenantId)
    {
        return await _context.MoveOutInspections
            .AnyAsync(m => m.TenantId == tenantId &&
                          (m.Status == MoveOutInspectionStatus.Scheduled ||
                           m.Status == MoveOutInspectionStatus.InProgress));
    }
}
