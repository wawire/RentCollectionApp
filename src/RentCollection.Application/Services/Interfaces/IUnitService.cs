using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Units;

namespace RentCollection.Application.Services.Interfaces;

public interface IUnitService
{
    Task<Result<IEnumerable<UnitDto>>> GetAllUnitsAsync();
    Task<Result<IEnumerable<UnitDto>>> GetUnitsByPropertyIdAsync(int propertyId);
    Task<Result<UnitDto>> GetUnitByIdAsync(int id);
    Task<Result<UnitDto>> CreateUnitAsync(CreateUnitDto createDto);
    Task<Result<UnitDto>> UpdateUnitAsync(int id, UpdateUnitDto updateDto);
    Task<Result> DeleteUnitAsync(int id);
}
