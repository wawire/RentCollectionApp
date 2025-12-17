using RentCollection.Domain.Entities;

namespace RentCollection.Application.Interfaces;

public interface ITenantRepository : IRepository<Tenant>
{
    Task<IEnumerable<Tenant>> GetTenantsByUnitIdAsync(int unitId);
    Task<Tenant?> GetTenantWithDetailsAsync(int id);
    Task<IEnumerable<Tenant>> GetActiveTenantsAsync();
    Task<IEnumerable<Tenant>> GetActiveTenantsWithFullDetailsAsync();
    Task<Tenant?> GetByEmailAsync(string email);
    Task<Tenant?> GetByPhoneNumberAsync(string phoneNumber);
}
