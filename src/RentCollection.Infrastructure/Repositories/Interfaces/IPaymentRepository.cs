using RentCollection.Domain.Entities;

namespace RentCollection.Infrastructure.Repositories.Interfaces;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<IEnumerable<Payment>> GetPaymentsByTenantIdAsync(int tenantId);
    Task<Payment?> GetPaymentWithDetailsAsync(int id);
    Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
}
