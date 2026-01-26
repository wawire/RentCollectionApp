using Microsoft.EntityFrameworkCore;
using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Utilities;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Services;

public class UtilityConfigService : IUtilityConfigService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UtilityConfigService(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<UtilityConfigDto>>> GetUtilityConfigsAsync(
        int? propertyId = null,
        int? unitId = null,
        bool includeInactive = false)
    {
        var query = _context.UtilityConfigs
            .Include(c => c.UtilityType)
            .Include(c => c.Property)
            .Include(c => c.Unit)
            .AsQueryable();

        query = ApplyScope(query);

        if (propertyId.HasValue)
        {
            query = query.Where(c => c.PropertyId == propertyId.Value);
        }

        if (unitId.HasValue)
        {
            query = query.Where(c => c.UnitId == unitId.Value);
        }

        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        var configs = await query.OrderBy(c => c.Property.Name).ThenBy(c => c.UnitId).ToListAsync();
        var dtos = configs.Select(MapToDto).ToList();

        return Result<List<UtilityConfigDto>>.Success(dtos);
    }

    public async Task<Result<UtilityConfigDto>> CreateUtilityConfigAsync(CreateUtilityConfigDto dto)
    {
        var validationError = ValidateConfig(dto.BillingMode, dto.FixedAmount, dto.Rate, dto.SharedAmount, dto.EffectiveFrom, dto.EffectiveTo);
        if (validationError != null)
        {
            return Result<UtilityConfigDto>.Failure(validationError);
        }

        var utilityType = await _context.UtilityTypes.FirstOrDefaultAsync(t => t.Id == dto.UtilityTypeId);
        if (utilityType == null)
        {
            return Result<UtilityConfigDto>.Failure("Utility type not found");
        }

        var property = await _context.Properties.Include(p => p.Landlord).FirstOrDefaultAsync(p => p.Id == dto.PropertyId);
        if (property == null)
        {
            return Result<UtilityConfigDto>.Failure("Property not found");
        }

        if (!IsPropertyInScope(property))
        {
            return Result<UtilityConfigDto>.Failure("You do not have access to this property");
        }

        Unit? unit = null;
        if (dto.UnitId.HasValue)
        {
            unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == dto.UnitId.Value && u.PropertyId == property.Id);
            if (unit == null)
            {
                return Result<UtilityConfigDto>.Failure("Unit not found for this property");
            }
        }

        var config = new UtilityConfig
        {
            UtilityTypeId = utilityType.Id,
            PropertyId = property.Id,
            UnitId = unit?.Id,
            BillingMode = dto.BillingMode,
            FixedAmount = dto.FixedAmount,
            Rate = dto.Rate,
            SharedAmount = dto.SharedAmount,
            EffectiveFrom = dto.EffectiveFrom,
            EffectiveTo = dto.EffectiveTo,
            Notes = dto.Notes
        };

        _context.UtilityConfigs.Add(config);
        await _context.SaveChangesAsync();

        config.UtilityType = utilityType;
        config.Property = property;
        config.Unit = unit;

        return Result<UtilityConfigDto>.Success(MapToDto(config));
    }

    public async Task<Result<UtilityConfigDto>> UpdateUtilityConfigAsync(int id, UpdateUtilityConfigDto dto)
    {
        var config = await _context.UtilityConfigs
            .Include(c => c.UtilityType)
            .Include(c => c.Property)
            .Include(c => c.Unit)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (config == null)
        {
            return Result<UtilityConfigDto>.Failure("Utility configuration not found");
        }

        if (!IsPropertyInScope(config.Property))
        {
            return Result<UtilityConfigDto>.Failure("You do not have access to this property");
        }

        var validationError = ValidateConfig(dto.BillingMode, dto.FixedAmount, dto.Rate, dto.SharedAmount, config.EffectiveFrom, dto.EffectiveTo);
        if (validationError != null)
        {
            return Result<UtilityConfigDto>.Failure(validationError);
        }

        config.BillingMode = dto.BillingMode;
        config.FixedAmount = dto.FixedAmount;
        config.Rate = dto.Rate;
        config.SharedAmount = dto.SharedAmount;
        config.IsActive = dto.IsActive;
        config.EffectiveTo = dto.EffectiveTo;
        config.Notes = dto.Notes;

        await _context.SaveChangesAsync();

        return Result<UtilityConfigDto>.Success(MapToDto(config));
    }

    private IQueryable<UtilityConfig> ApplyScope(IQueryable<UtilityConfig> query)
    {
        if (_currentUserService.IsPlatformAdmin)
        {
            return query;
        }

        if (_currentUserService.IsLandlord && _currentUserService.UserIdInt.HasValue)
        {
            return query.Where(c => c.Property.LandlordId == _currentUserService.UserIdInt.Value);
        }

        if (_currentUserService.IsCaretaker || _currentUserService.IsManager || _currentUserService.IsAccountant)
        {
            var assignedPropertyIds = _currentUserService.GetAssignedPropertyIdsAsync().GetAwaiter().GetResult();
            return assignedPropertyIds.Count == 0
                ? query.Where(_ => false)
                : query.Where(c => assignedPropertyIds.Contains(c.PropertyId));
        }

        return query.Where(_ => false);
    }

    private bool IsPropertyInScope(Property property)
    {
        if (_currentUserService.IsPlatformAdmin)
        {
            return true;
        }

        if (_currentUserService.IsLandlord && _currentUserService.UserIdInt.HasValue)
        {
            return property.LandlordId == _currentUserService.UserIdInt.Value;
        }

        if (_currentUserService.IsCaretaker || _currentUserService.IsManager || _currentUserService.IsAccountant)
        {
            var assignedPropertyIds = _currentUserService.GetAssignedPropertyIdsAsync().GetAwaiter().GetResult();
            return assignedPropertyIds.Contains(property.Id);
        }

        return false;
    }

    private static UtilityConfigDto MapToDto(UtilityConfig config)
    {
        return new UtilityConfigDto
        {
            Id = config.Id,
            UtilityTypeId = config.UtilityTypeId,
            UtilityTypeName = config.UtilityType?.Name ?? string.Empty,
            PropertyId = config.PropertyId,
            PropertyName = config.Property?.Name ?? string.Empty,
            UnitId = config.UnitId,
            UnitNumber = config.Unit?.UnitNumber,
            BillingMode = config.BillingMode,
            FixedAmount = config.FixedAmount,
            Rate = config.Rate,
            SharedAmount = config.SharedAmount,
            IsActive = config.IsActive,
            EffectiveFrom = config.EffectiveFrom,
            EffectiveTo = config.EffectiveTo,
            Notes = config.Notes
        };
    }

    private static string? ValidateConfig(
        UtilityBillingMode billingMode,
        decimal? fixedAmount,
        decimal? rate,
        decimal? sharedAmount,
        DateTime effectiveFrom,
        DateTime? effectiveTo)
    {
        if (effectiveTo.HasValue && effectiveTo.Value.Date < effectiveFrom.Date)
        {
            return "Effective end date cannot be before the start date";
        }

        if (fixedAmount.HasValue && fixedAmount.Value < 0)
        {
            return "Fixed amount cannot be negative";
        }

        if (rate.HasValue && rate.Value < 0)
        {
            return "Rate cannot be negative";
        }

        if (sharedAmount.HasValue && sharedAmount.Value < 0)
        {
            return "Shared amount cannot be negative";
        }

        return billingMode switch
        {
            UtilityBillingMode.Fixed when !fixedAmount.HasValue || fixedAmount.Value <= 0 =>
                "Fixed amount is required for fixed utilities",
            UtilityBillingMode.Shared when !sharedAmount.HasValue || sharedAmount.Value <= 0 =>
                "Shared amount is required for shared utilities",
            UtilityBillingMode.Metered when !rate.HasValue || rate.Value <= 0 =>
                "Rate is required for metered utilities",
            _ => null
        };
    }
}

