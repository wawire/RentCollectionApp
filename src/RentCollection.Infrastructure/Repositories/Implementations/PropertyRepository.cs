using Microsoft.EntityFrameworkCore;
using RentCollection.Domain.Entities;
using RentCollection.Infrastructure.Data;
using RentCollection.Infrastructure.Repositories.Interfaces;

namespace RentCollection.Infrastructure.Repositories.Implementations;

public class PropertyRepository : Repository<Property>, IPropertyRepository
{
    public PropertyRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Property>> GetPropertiesWithUnitsAsync()
    {
        return await _context.Properties
            .Include(p => p.Units)
            .ToListAsync();
    }

    public async Task<Property?> GetPropertyWithUnitsAsync(int id)
    {
        return await _context.Properties
            .Include(p => p.Units)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
