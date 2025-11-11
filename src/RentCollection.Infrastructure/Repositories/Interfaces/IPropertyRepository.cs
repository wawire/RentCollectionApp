using RentCollection.Domain.Entities;

namespace RentCollection.Infrastructure.Repositories.Interfaces;

public interface IPropertyRepository : IRepository<Property>
{
    Task<IEnumerable<Property>> GetPropertiesWithUnitsAsync();
    Task<Property?> GetPropertyWithUnitsAsync(int id);
}
