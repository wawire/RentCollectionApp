using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Helpers;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Services;

public class InvoiceBalanceService : IInvoiceBalanceService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InvoiceBalanceService> _logger;

    public InvoiceBalanceService(ApplicationDbContext context, ILogger<InvoiceBalanceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<decimal> GetOutstandingBalanceForTenantAsync(int tenantId, CancellationToken cancellationToken = default)
    {
        var invoices = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Allocations)
            .Where(i => i.TenantId == tenantId && i.Status != InvoiceStatus.Void)
            .ToListAsync(cancellationToken);

        var total = 0m;
        var now = DateTime.UtcNow;

        foreach (var invoice in invoices)
        {
            var allocated = invoice.Allocations.Sum(a => a.Amount);
            var balance = InvoiceStatusCalculator.CalculateBalance(invoice, allocated);
            if (balance > 0)
            {
                total += balance;
            }
        }

        return total;
    }

    public async Task<int> RecalculateAllAsync(CancellationToken cancellationToken = default)
    {
        var invoices = await _context.Invoices
            .Include(i => i.Allocations)
            .ToListAsync(cancellationToken);

        return await RecalculateInternalAsync(invoices, cancellationToken);
    }

    public async Task<int> RecalculateForTenantAsync(int tenantId, CancellationToken cancellationToken = default)
    {
        var invoices = await _context.Invoices
            .Include(i => i.Allocations)
            .Where(i => i.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        return await RecalculateInternalAsync(invoices, cancellationToken);
    }

    public async Task<int> RecalculateForInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        var invoices = await _context.Invoices
            .Include(i => i.Allocations)
            .Where(i => i.Id == invoiceId)
            .ToListAsync(cancellationToken);

        return await RecalculateInternalAsync(invoices, cancellationToken);
    }

    private async Task<int> RecalculateInternalAsync(List<Domain.Entities.Invoice> invoices, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var updated = 0;

        foreach (var invoice in invoices)
        {
            var allocated = invoice.Allocations.Sum(a => a.Amount);
            if (InvoiceStatusCalculator.Apply(invoice, allocated, now))
            {
                updated += 1;
            }
        }

        if (updated > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("Recalculated invoice balances for {Count} invoices", updated);
        return updated;
    }
}
