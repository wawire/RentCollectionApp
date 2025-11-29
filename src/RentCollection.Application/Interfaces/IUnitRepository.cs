using RentCollection.Domain.Entities;

namespace RentCollection.Application.Interfaces;

public interface IUnitRepository : IRepository<Unit>
{
    Task<IEnumerable<Unit>> GetUnitsByPropertyIdAsync(int propertyId);
    Task<Unit?> GetUnitWithDetailsAsync(int id);
}
