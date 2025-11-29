using RentCollection.Domain.Entities;

namespace RentCollection.Application.Interfaces;

public interface IPropertyRepository : IRepository<Property>
{
    Task<IEnumerable<Property>> GetPropertiesWithUnitsAsync();
    Task<Property?> GetPropertyWithUnitsAsync(int id);
}
