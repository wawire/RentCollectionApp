using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Utilities;

namespace RentCollection.Application.Services.Interfaces;

public interface IUtilityTypeService
{
    Task<Result<List<UtilityTypeDto>>> GetUtilityTypesAsync(bool includeInactive = false);
    Task<Result<UtilityTypeDto>> CreateUtilityTypeAsync(CreateUtilityTypeDto dto);
}
