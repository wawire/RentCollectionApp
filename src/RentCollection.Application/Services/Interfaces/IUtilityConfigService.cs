using RentCollection.Application.DTOs.Utilities;
using RentCollection.Application.Common.Models;

namespace RentCollection.Application.Services.Interfaces;

public interface IUtilityConfigService
{
    Task<Result<List<UtilityConfigDto>>> GetUtilityConfigsAsync(int? propertyId = null, int? unitId = null, bool includeInactive = false);
    Task<Result<UtilityConfigDto>> CreateUtilityConfigAsync(CreateUtilityConfigDto dto);
    Task<Result<UtilityConfigDto>> UpdateUtilityConfigAsync(int id, UpdateUtilityConfigDto dto);
}
