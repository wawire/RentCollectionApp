using RentCollection.Domain.Entities;

namespace RentCollection.Infrastructure.Repositories.Interfaces;

public interface ITenantRepository : IRepository<Tenant>
{
    Task<IEnumerable<Tenant>> GetTenantsByUnitIdAsync(int unitId);
    Task<Tenant?> GetTenantWithDetailsAsync(int id);
    Task<IEnumerable<Tenant>> GetActiveTenantsAsync();
}
