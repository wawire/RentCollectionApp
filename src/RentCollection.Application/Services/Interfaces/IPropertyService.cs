using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Properties;

namespace RentCollection.Application.Services.Interfaces;

public interface IPropertyService
{
    Task<Result<IEnumerable<PropertyDto>>> GetAllPropertiesAsync();
    Task<Result<PropertyDto>> GetPropertyByIdAsync(int id);
    Task<Result<PropertyDto>> CreatePropertyAsync(CreatePropertyDto createDto);
    Task<Result<PropertyDto>> UpdatePropertyAsync(int id, UpdatePropertyDto updateDto);
    Task<Result> DeletePropertyAsync(int id);
    Task<Result<PaginatedList<PropertyDto>>> GetPropertiesPaginatedAsync(int pageNumber, int pageSize);
}
