using RentCollection.Domain.Entities;

namespace RentCollection.Application.Services.Interfaces;

public interface IUtilityBillingService
{
    Task<List<InvoiceLineItem>> BuildLineItemsForTenantAsync(Tenant tenant, DateTime periodStart, DateTime periodEnd);
}
