using Microsoft.EntityFrameworkCore;
using RentCollection.Application.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Repositories.Implementations;

public class SecurityDepositRepository : Repository<SecurityDepositTransaction>, ISecurityDepositRepository
{
    public SecurityDepositRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<SecurityDepositTransaction>> GetByTenantIdAsync(int tenantId)
    {
        return await _context.SecurityDepositTransactions
            .Include(t => t.Tenant)
            .Include(t => t.CreatedByUser)
            .Include(t => t.RelatedPayment)
            .Include(t => t.RelatedMaintenanceRequest)
            .Where(t => t.TenantId == tenantId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<decimal> GetCurrentBalanceAsync(int tenantId)
    {
        var transactions = await _context.SecurityDepositTransactions
            .Where(t => t.TenantId == tenantId)
            .ToListAsync();

        var initialDeposit = transactions
            .Where(t => t.TransactionType == SecurityDepositTransactionType.Initial)
            .Sum(t => t.Amount);

        var deductions = transactions
            .Where(t => t.TransactionType == SecurityDepositTransactionType.Deduction)
            .Sum(t => t.Amount);

        var refunds = transactions
            .Where(t => t.TransactionType == SecurityDepositTransactionType.Refund)
            .Sum(t => t.Amount);

        var adjustments = transactions
            .Where(t => t.TransactionType == SecurityDepositTransactionType.Adjustment)
            .Sum(t => t.Amount);

        return initialDeposit - deductions - refunds + adjustments;
    }

    public async Task<SecurityDepositTransaction?> GetInitialDepositAsync(int tenantId)
    {
        return await _context.SecurityDepositTransactions
            .Include(t => t.Tenant)
            .Include(t => t.CreatedByUser)
            .Where(t => t.TenantId == tenantId && t.TransactionType == SecurityDepositTransactionType.Initial)
            .OrderBy(t => t.TransactionDate)
            .FirstOrDefaultAsync();
    }

    public async Task<List<SecurityDepositTransaction>> GetByTypeAsync(int tenantId, SecurityDepositTransactionType type)
    {
        return await _context.SecurityDepositTransactions
            .Include(t => t.Tenant)
            .Include(t => t.CreatedByUser)
            .Where(t => t.TenantId == tenantId && t.TransactionType == type)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<List<SecurityDepositTransaction>> GetTransactionHistoryAsync(int tenantId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.SecurityDepositTransactions
            .Include(t => t.Tenant)
            .Include(t => t.CreatedByUser)
            .Include(t => t.RelatedPayment)
            .Include(t => t.RelatedMaintenanceRequest)
            .Where(t => t.TenantId == tenantId);

        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);

        return await query
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }
}
