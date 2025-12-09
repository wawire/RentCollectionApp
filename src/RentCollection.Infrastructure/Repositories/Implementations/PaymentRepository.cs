using Microsoft.EntityFrameworkCore;
using RentCollection.Application.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Repositories.Implementations;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByTenantIdAsync(int tenantId)
    {
        return await _context.Payments
            .Include(p => p.Tenant)
                .ThenInclude(t => t.Unit)
                    .ThenInclude(u => u.Property)
            .Where(p => p.TenantId == tenantId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<Payment?> GetPaymentWithDetailsAsync(int id)
    {
        return await _context.Payments
            .Include(p => p.Tenant)
                .ThenInclude(t => t.Unit)
                    .ThenInclude(u => u.Property)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Payments
            .Include(p => p.Tenant)
                .ThenInclude(t => t.Unit)
                    .ThenInclude(u => u.Property)
            .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public override async Task<IEnumerable<Payment>> GetAllAsync()
    {
        return await _context.Payments
            .Include(p => p.Tenant)
                .ThenInclude(t => t.Unit)
                    .ThenInclude(u => u.Property)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public override async Task<Payment?> GetByIdAsync(int id)
    {
        return await _context.Payments
            .Include(p => p.Tenant)
                .ThenInclude(t => t.Unit)
                    .ThenInclude(u => u.Property)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Payment>> GetOverduePaymentsAsync()
    {
        var today = DateTime.UtcNow.Date;

        return await _context.Payments
            .Include(p => p.Tenant)
                .ThenInclude(t => t.Unit)
                    .ThenInclude(u => u.Property)
            .Where(p => p.Status == PaymentStatus.Pending && p.DueDate.Date < today)
            .OrderBy(p => p.DueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetOverduePaymentsByLandlordIdAsync(int landlordId)
    {
        var today = DateTime.UtcNow.Date;

        return await _context.Payments
            .Include(p => p.Tenant)
                .ThenInclude(t => t.Unit)
                    .ThenInclude(u => u.Property)
            .Where(p => p.Status == PaymentStatus.Pending
                     && p.DueDate.Date < today
                     && p.Tenant.Unit.Property.LandlordId == landlordId)
            .OrderBy(p => p.DueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetOverduePaymentsByPropertyIdAsync(int propertyId)
    {
        var today = DateTime.UtcNow.Date;

        return await _context.Payments
            .Include(p => p.Tenant)
                .ThenInclude(t => t.Unit)
                    .ThenInclude(u => u.Property)
            .Where(p => p.Status == PaymentStatus.Pending
                     && p.DueDate.Date < today
                     && p.Tenant.Unit.PropertyId == propertyId)
            .OrderBy(p => p.DueDate)
            .ToListAsync();
    }
}
