using Microsoft.EntityFrameworkCore;
using RentCollection.Application.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Repositories.Implementations;

public class TenantRepository : Repository<Tenant>, ITenantRepository
{
    public TenantRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Tenant>> GetTenantsByUnitIdAsync(int unitId)
    {
        return await _context.Tenants
            .Include(t => t.Unit)
                .ThenInclude(u => u.Property)
            .Include(t => t.Payments)
            .Where(t => t.UnitId == unitId)
            .OrderByDescending(t => t.IsActive)
            .ThenByDescending(t => t.LeaseStartDate)
            .ToListAsync();
    }

    public async Task<Tenant?> GetTenantWithDetailsAsync(int id)
    {
        return await _context.Tenants
            .Include(t => t.Unit)
                .ThenInclude(u => u.Property)
            .Include(t => t.Payments.OrderByDescending(p => p.PaymentDate))
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Tenant>> GetActiveTenantsAsync()
    {
        return await _context.Tenants
            .Include(t => t.Unit)
                .ThenInclude(u => u.Property)
            .Where(t => t.IsActive)
            .OrderBy(t => t.Unit.Property.Name)
            .ThenBy(t => t.Unit.UnitNumber)
            .ThenBy(t => t.LastName)
            .ToListAsync();
    }

    public override async Task<IEnumerable<Tenant>> GetAllAsync()
    {
        return await _context.Tenants
            .Include(t => t.Unit)
                .ThenInclude(u => u.Property)
            .OrderByDescending(t => t.IsActive)
            .ThenBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .ToListAsync();
    }

    public override async Task<Tenant?> GetByIdAsync(int id)
    {
        return await _context.Tenants
            .Include(t => t.Unit)
                .ThenInclude(u => u.Property)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tenant?> GetByEmailAsync(string email)
    {
        return await _context.Tenants
            .Include(t => t.Unit)
                .ThenInclude(u => u.Property)
            .FirstOrDefaultAsync(t => t.Email == email);
    }

    public async Task<Tenant?> GetByPhoneNumberAsync(string phoneNumber)
    {
        return await _context.Tenants
            .Include(t => t.Unit)
                .ThenInclude(u => u.Property)
            .FirstOrDefaultAsync(t => t.PhoneNumber == phoneNumber);
    }
}
