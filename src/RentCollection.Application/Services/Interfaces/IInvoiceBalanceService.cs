namespace RentCollection.Application.Services.Interfaces;

public interface IInvoiceBalanceService
{
    Task<decimal> GetOutstandingBalanceForTenantAsync(int tenantId, CancellationToken cancellationToken = default);
    Task<int> RecalculateAllAsync(CancellationToken cancellationToken = default);
    Task<int> RecalculateForTenantAsync(int tenantId, CancellationToken cancellationToken = default);
    Task<int> RecalculateForInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default);
}
