using RentCollection.Domain.Entities;

namespace RentCollection.Application.Interfaces;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<IEnumerable<Payment>> GetPaymentsByTenantIdAsync(int tenantId);
    Task<Payment?> GetPaymentWithDetailsAsync(int id);
    Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Payment>> GetOverduePaymentsAsync();
    Task<IEnumerable<Payment>> GetOverduePaymentsByLandlordIdAsync(int landlordId);
    Task<IEnumerable<Payment>> GetOverduePaymentsByPropertyIdAsync(int propertyId);
    Task<IEnumerable<Payment>> GetOverduePaymentsByPropertyIdsAsync(IEnumerable<int> propertyIds);
    Task<bool> HasConfirmedPaymentAsync(int tenantId, int month, int year);
}
