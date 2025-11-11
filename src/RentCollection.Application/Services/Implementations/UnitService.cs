using AutoMapper;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Units;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Infrastructure.Repositories.Interfaces;

namespace RentCollection.Application.Services.Implementations;

public class UnitService : IUnitService
{
    private readonly IUnitRepository _unitRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UnitService> _logger;

    public UnitService(
        IUnitRepository unitRepository,
        IPropertyRepository propertyRepository,
        IMapper mapper,
        ILogger<UnitService> logger)
    {
        _unitRepository = unitRepository;
        _propertyRepository = propertyRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<UnitDto>>> GetAllUnitsAsync()
    {
        try
        {
            var units = await _unitRepository.GetAllAsync();
            var unitDtos = _mapper.Map<IEnumerable<UnitDto>>(units);

            return Result<IEnumerable<UnitDto>>.Success(unitDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all units");
            return Result<IEnumerable<UnitDto>>.Failure("An error occurred while retrieving units");
        }
    }

    public async Task<Result<IEnumerable<UnitDto>>> GetUnitsByPropertyIdAsync(int propertyId)
    {
        try
        {
            var property = await _propertyRepository.GetByIdAsync(propertyId);
            if (property == null)
            {
                return Result<IEnumerable<UnitDto>>.Failure($"Property with ID {propertyId} not found");
            }

            var units = await _unitRepository.GetUnitsByPropertyIdAsync(propertyId);
            var unitDtos = _mapper.Map<IEnumerable<UnitDto>>(units);

            return Result<IEnumerable<UnitDto>>.Success(unitDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving units for property {PropertyId}", propertyId);
            return Result<IEnumerable<UnitDto>>.Failure("An error occurred while retrieving units");
        }
    }

    public async Task<Result<UnitDto>> GetUnitByIdAsync(int id)
    {
        try
        {
            var unit = await _unitRepository.GetUnitWithDetailsAsync(id);

            if (unit == null)
            {
                return Result<UnitDto>.Failure($"Unit with ID {id} not found");
            }

            var unitDto = _mapper.Map<UnitDto>(unit);
            return Result<UnitDto>.Success(unitDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unit with ID {UnitId}", id);
            return Result<UnitDto>.Failure("An error occurred while retrieving the unit");
        }
    }

    public async Task<Result<UnitDto>> CreateUnitAsync(CreateUnitDto createDto)
    {
        try
        {
            // Validate property exists
            var property = await _propertyRepository.GetByIdAsync(createDto.PropertyId);
            if (property == null)
            {
                return Result<UnitDto>.Failure($"Property with ID {createDto.PropertyId} not found");
            }

            // Check for duplicate unit number in the same property
            var existingUnits = await _unitRepository.GetUnitsByPropertyIdAsync(createDto.PropertyId);
            if (existingUnits.Any(u => u.UnitNumber.Equals(createDto.UnitNumber, StringComparison.OrdinalIgnoreCase)))
            {
                return Result<UnitDto>.Failure($"Unit number '{createDto.UnitNumber}' already exists in this property");
            }

            var unit = _mapper.Map<Unit>(createDto);
            unit.CreatedAt = DateTime.UtcNow;
            unit.IsOccupied = false;
            unit.IsActive = true;

            var createdUnit = await _unitRepository.AddAsync(unit);

            // Reload with details
            var unitWithDetails = await _unitRepository.GetUnitWithDetailsAsync(createdUnit.Id);
            var unitDto = _mapper.Map<UnitDto>(unitWithDetails);

            _logger.LogInformation("Unit created successfully: {UnitNumber} in property {PropertyId}",
                createdUnit.UnitNumber, createDto.PropertyId);
            return Result<UnitDto>.Success(unitDto, "Unit created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating unit");
            return Result<UnitDto>.Failure("An error occurred while creating the unit");
        }
    }

    public async Task<Result<UnitDto>> UpdateUnitAsync(int id, UpdateUnitDto updateDto)
    {
        try
        {
            var existingUnit = await _unitRepository.GetUnitWithDetailsAsync(id);

            if (existingUnit == null)
            {
                return Result<UnitDto>.Failure($"Unit with ID {id} not found");
            }

            // Check for duplicate unit number if changing
            if (!existingUnit.UnitNumber.Equals(updateDto.UnitNumber, StringComparison.OrdinalIgnoreCase))
            {
                var unitsInProperty = await _unitRepository.GetUnitsByPropertyIdAsync(existingUnit.PropertyId);
                if (unitsInProperty.Any(u => u.Id != id && u.UnitNumber.Equals(updateDto.UnitNumber, StringComparison.OrdinalIgnoreCase)))
                {
                    return Result<UnitDto>.Failure($"Unit number '{updateDto.UnitNumber}' already exists in this property");
                }
            }

            _mapper.Map(updateDto, existingUnit);
            existingUnit.UpdatedAt = DateTime.UtcNow;

            await _unitRepository.UpdateAsync(existingUnit);

            var unitDto = _mapper.Map<UnitDto>(existingUnit);

            _logger.LogInformation("Unit updated successfully: {UnitId}", id);
            return Result<UnitDto>.Success(unitDto, "Unit updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating unit with ID {UnitId}", id);
            return Result<UnitDto>.Failure("An error occurred while updating the unit");
        }
    }

    public async Task<Result> DeleteUnitAsync(int id)
    {
        try
        {
            var unit = await _unitRepository.GetUnitWithDetailsAsync(id);

            if (unit == null)
            {
                return Result.Failure($"Unit with ID {id} not found");
            }

            // Check if unit has active tenants
            if (unit.Tenants.Any(t => t.IsActive))
            {
                return Result.Failure("Cannot delete unit with active tenants. Please deactivate or move tenants first.");
            }

            await _unitRepository.DeleteAsync(unit);

            _logger.LogInformation("Unit deleted successfully: {UnitId}", id);
            return Result.Success("Unit deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting unit with ID {UnitId}", id);
            return Result.Failure("An error occurred while deleting the unit");
        }
    }
}
