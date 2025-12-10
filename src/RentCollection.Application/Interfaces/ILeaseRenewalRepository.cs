using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Interfaces
{
    public interface ILeaseRenewalRepository : IRepository<LeaseRenewal>
    {
        Task<List<LeaseRenewal>> GetByTenantIdAsync(int tenantId);
        Task<List<LeaseRenewal>> GetByPropertyIdAsync(int propertyId);
        Task<List<LeaseRenewal>> GetByStatusAsync(LeaseRenewalStatus status);
        Task<List<LeaseRenewal>> GetExpiringSoonAsync(int daysUntilExpiry = 60);
        Task<LeaseRenewal?> GetWithDetailsAsync(int id);
        Task<LeaseRenewal?> GetActiveRenewalForTenantAsync(int tenantId);
    }
}
