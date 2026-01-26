using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Utilities;

namespace RentCollection.Application.Services.Interfaces;

public interface IMeterReadingService
{
    Task<Result<List<MeterReadingDto>>> GetMeterReadingsAsync(int? propertyId = null, int? unitId = null, int? utilityConfigId = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<Result<MeterReadingDto>> CreateMeterReadingAsync(CreateMeterReadingDto dto);
}
