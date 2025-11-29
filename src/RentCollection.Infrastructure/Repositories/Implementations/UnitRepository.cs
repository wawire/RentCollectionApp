using Microsoft.EntityFrameworkCore;
using RentCollection.Application.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Repositories.Implementations;

public class UnitRepository : Repository<Unit>, IUnitRepository
{
    public UnitRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Unit>> GetUnitsByPropertyIdAsync(int propertyId)
    {
        return await _context.Units
            .Include(u => u.Property)
            .Include(u => u.Tenants.Where(t => t.IsActive))
            .Where(u => u.PropertyId == propertyId)
            .OrderBy(u => u.UnitNumber)
            .ToListAsync();
    }

    public async Task<Unit?> GetUnitWithDetailsAsync(int id)
    {
        return await _context.Units
            .Include(u => u.Property)
            .Include(u => u.Tenants.Where(t => t.IsActive))
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public override async Task<IEnumerable<Unit>> GetAllAsync()
    {
        return await _context.Units
            .Include(u => u.Property)
            .Include(u => u.Tenants.Where(t => t.IsActive))
            .OrderBy(u => u.Property.Name)
            .ThenBy(u => u.UnitNumber)
            .ToListAsync();
    }

    public override async Task<Unit?> GetByIdAsync(int id)
    {
        return await _context.Units
            .Include(u => u.Property)
            .Include(u => u.Tenants.Where(t => t.IsActive))
            .FirstOrDefaultAsync(u => u.Id == id);
    }
}
