using Microsoft.EntityFrameworkCore;
using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Utilities;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Services;

public class MeterReadingService : IMeterReadingService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public MeterReadingService(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<MeterReadingDto>>> GetMeterReadingsAsync(
        int? propertyId = null,
        int? unitId = null,
        int? utilityConfigId = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var query = _context.MeterReadings
            .Include(r => r.UtilityConfig)
                .ThenInclude(c => c.UtilityType)
            .Include(r => r.Unit)
                .ThenInclude(u => u.Property)
            .AsQueryable();

        query = await ApplyScopeAsync(query);

        if (propertyId.HasValue)
        {
            query = query.Where(r => r.Unit.PropertyId == propertyId.Value);
        }

        if (unitId.HasValue)
        {
            query = query.Where(r => r.UnitId == unitId.Value);
        }

        if (utilityConfigId.HasValue)
        {
            query = query.Where(r => r.UtilityConfigId == utilityConfigId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(r => r.ReadingDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(r => r.ReadingDate <= endDate.Value);
        }

        var readings = await query
            .OrderByDescending(r => r.ReadingDate)
            .ToListAsync();

        var dtos = readings.Select(r => new MeterReadingDto
        {
            Id = r.Id,
            UtilityConfigId = r.UtilityConfigId,
            UnitId = r.UnitId,
            UnitNumber = r.Unit.UnitNumber,
            UtilityName = r.UtilityConfig.UtilityType.Name,
            ReadingDate = r.ReadingDate,
            ReadingValue = r.ReadingValue,
            PhotoUrl = r.PhotoUrl,
            Notes = r.Notes
        }).ToList();

        return Result<List<MeterReadingDto>>.Success(dtos);
    }

    public async Task<Result<MeterReadingDto>> CreateMeterReadingAsync(CreateMeterReadingDto dto)
    {
        var config = await _context.UtilityConfigs
            .Include(c => c.UtilityType)
            .Include(c => c.Property)
            .FirstOrDefaultAsync(c => c.Id == dto.UtilityConfigId);

        if (config == null)
        {
            return Result<MeterReadingDto>.Failure("Utility configuration not found");
        }

        if (config.BillingMode != UtilityBillingMode.Metered)
        {
            return Result<MeterReadingDto>.Failure("Meter readings can only be recorded for metered utilities");
        }

        if (!await IsPropertyInScopeAsync(config.Property))
        {
            return Result<MeterReadingDto>.Failure("You do not have access to this property");
        }

        var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == dto.UnitId && u.PropertyId == config.PropertyId);
        if (unit == null)
        {
            return Result<MeterReadingDto>.Failure("Unit not found for this property");
        }

        var lastReading = await _context.MeterReadings
            .Where(r => r.UtilityConfigId == config.Id && r.UnitId == unit.Id)
            .OrderByDescending(r => r.ReadingDate)
            .FirstOrDefaultAsync();

        if (lastReading != null && dto.ReadingValue < lastReading.ReadingValue)
        {
            return Result<MeterReadingDto>.Failure("Reading value cannot be less than the previous reading");
        }

        var reading = new MeterReading
        {
            UtilityConfigId = config.Id,
            UnitId = unit.Id,
            ReadingDate = dto.ReadingDate,
            ReadingValue = dto.ReadingValue,
            PhotoUrl = dto.PhotoUrl,
            Notes = dto.Notes,
            RecordedByUserId = _currentUserService.UserIdInt
        };

        _context.MeterReadings.Add(reading);
        await _context.SaveChangesAsync();

        return Result<MeterReadingDto>.Success(new MeterReadingDto
        {
            Id = reading.Id,
            UtilityConfigId = reading.UtilityConfigId,
            UnitId = reading.UnitId,
            UnitNumber = unit.UnitNumber,
            UtilityName = config.UtilityType.Name,
            ReadingDate = reading.ReadingDate,
            ReadingValue = reading.ReadingValue,
            PhotoUrl = reading.PhotoUrl,
            Notes = reading.Notes
        });
    }

    private async Task<IQueryable<MeterReading>> ApplyScopeAsync(IQueryable<MeterReading> query)
    {
        if (_currentUserService.IsPlatformAdmin)
        {
            return query;
        }

        if (_currentUserService.IsLandlord && _currentUserService.UserIdInt.HasValue)
        {
            return query.Where(r => r.Unit.Property.LandlordId == _currentUserService.UserIdInt.Value);
        }

        if (_currentUserService.IsCaretaker && _currentUserService.PropertyId.HasValue)
        {
            return query.Where(r => r.Unit.PropertyId == _currentUserService.PropertyId.Value);
        }

        if (_currentUserService.IsManager || _currentUserService.IsCaretaker || _currentUserService.IsAccountant)
        {
            var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
            if (assignedPropertyIds.Count > 0)
            {
                return query.Where(r => assignedPropertyIds.Contains(r.Unit.PropertyId));
            }
        }

        return query.Where(_ => false);
    }

    private async Task<bool> IsPropertyInScopeAsync(Property property)
    {
        if (_currentUserService.IsPlatformAdmin)
        {
            return true;
        }

        if (_currentUserService.IsLandlord && _currentUserService.UserIdInt.HasValue)
        {
            return property.LandlordId == _currentUserService.UserIdInt.Value;
        }

        if (_currentUserService.IsCaretaker && _currentUserService.PropertyId.HasValue)
        {
            return property.Id == _currentUserService.PropertyId.Value;
        }

        if (_currentUserService.IsManager || _currentUserService.IsCaretaker || _currentUserService.IsAccountant)
        {
            var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
            return assignedPropertyIds.Contains(property.Id);
        }

        return false;
    }
}

