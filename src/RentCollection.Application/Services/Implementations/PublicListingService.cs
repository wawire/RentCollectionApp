using AutoMapper;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Public;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.Application.Services.Implementations;

public class PublicListingService : IPublicListingService
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUnitRepository _unitRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PublicListingService> _logger;

    public PublicListingService(
        IPropertyRepository propertyRepository,
        IUnitRepository unitRepository,
        IMapper mapper,
        ILogger<PublicListingService> logger)
    {
        _propertyRepository = propertyRepository;
        _unitRepository = unitRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<PublicPropertyListingDto>>> GetPublicPropertiesAsync()
    {
        try
        {
            var properties = await _propertyRepository.GetPropertiesWithUnitsAsync();

            // Only include properties with vacant units
            var propertiesWithVacancies = properties
                .Where(p => p.Units.Any(u => !u.IsOccupied && u.IsActive))
                .Select(p => new PublicPropertyListingDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Address = p.Address,
                    City = p.City,
                    TotalUnits = p.Units.Count,
                    VacantUnits = p.Units.Count(u => !u.IsOccupied && u.IsActive),
                    AvailableUnits = p.Units
                        .Where(u => !u.IsOccupied && u.IsActive)
                        .Select(u => new PublicUnitListingDto
                        {
                            Id = u.Id,
                            UnitNumber = u.UnitNumber,
                            UnitType = u.UnitType,
                            Bedrooms = u.Bedrooms,
                            Bathrooms = u.Bathrooms,
                            Size = u.Size,
                            MonthlyRent = u.MonthlyRent,
                            Description = u.Description,
                            IsOccupied = u.IsOccupied,
                            PropertyId = p.Id,
                            PropertyName = p.Name,
                            PropertyAddress = p.Address,
                            PropertyCity = p.City
                        })
                        .ToList()
                })
                .ToList();

            return Result<IEnumerable<PublicPropertyListingDto>>.Success(propertiesWithVacancies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving public property listings");
            return Result<IEnumerable<PublicPropertyListingDto>>.Failure("An error occurred while retrieving property listings");
        }
    }

    public async Task<Result<PublicPropertyListingDto>> GetPublicPropertyByIdAsync(int propertyId)
    {
        try
        {
            var property = await _propertyRepository.GetPropertyWithUnitsAsync(propertyId);

            if (property == null)
            {
                return Result<PublicPropertyListingDto>.Failure($"Property with ID {propertyId} not found");
            }

            var dto = new PublicPropertyListingDto
            {
                Id = property.Id,
                Name = property.Name,
                Description = property.Description,
                Address = property.Address,
                City = property.City,
                TotalUnits = property.Units.Count,
                VacantUnits = property.Units.Count(u => !u.IsOccupied && u.IsActive),
                AvailableUnits = property.Units
                    .Where(u => !u.IsOccupied && u.IsActive)
                    .Select(u => new PublicUnitListingDto
                    {
                        Id = u.Id,
                        UnitNumber = u.UnitNumber,
                        UnitType = u.UnitType,
                        Bedrooms = u.Bedrooms,
                        Bathrooms = u.Bathrooms,
                        Size = u.Size,
                        MonthlyRent = u.MonthlyRent,
                        Description = u.Description,
                        IsOccupied = u.IsOccupied,
                        PropertyId = property.Id,
                        PropertyName = property.Name,
                        PropertyAddress = property.Address,
                        PropertyCity = property.City
                    })
                    .ToList()
            };

            return Result<PublicPropertyListingDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving public property details for property {PropertyId}", propertyId);
            return Result<PublicPropertyListingDto>.Failure("An error occurred while retrieving property details");
        }
    }

    public async Task<Result<IEnumerable<PublicUnitListingDto>>> GetVacantUnitsAsync()
    {
        try
        {
            var units = await _unitRepository.GetAllAsync();

            var vacantUnits = units
                .Where(u => !u.IsOccupied && u.IsActive)
                .Select(u => new PublicUnitListingDto
                {
                    Id = u.Id,
                    UnitNumber = u.UnitNumber,
                    UnitType = u.UnitType,
                    Bedrooms = u.Bedrooms,
                    Bathrooms = u.Bathrooms,
                    Size = u.Size,
                    MonthlyRent = u.MonthlyRent,
                    Description = u.Description,
                    IsOccupied = u.IsOccupied,
                    PropertyId = u.PropertyId,
                    PropertyName = u.Property?.Name,
                    PropertyAddress = u.Property?.Address,
                    PropertyCity = u.Property?.City
                })
                .ToList();

            return Result<IEnumerable<PublicUnitListingDto>>.Success(vacantUnits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vacant units");
            return Result<IEnumerable<PublicUnitListingDto>>.Failure("An error occurred while retrieving vacant units");
        }
    }

    public async Task<Result<IEnumerable<PublicUnitListingDto>>> GetVacantUnitsByPropertyAsync(int propertyId)
    {
        try
        {
            var property = await _propertyRepository.GetPropertyWithUnitsAsync(propertyId);

            if (property == null)
            {
                return Result<IEnumerable<PublicUnitListingDto>>.Failure($"Property with ID {propertyId} not found");
            }

            var vacantUnits = property.Units
                .Where(u => !u.IsOccupied && u.IsActive)
                .Select(u => new PublicUnitListingDto
                {
                    Id = u.Id,
                    UnitNumber = u.UnitNumber,
                    UnitType = u.UnitType,
                    Bedrooms = u.Bedrooms,
                    Bathrooms = u.Bathrooms,
                    Size = u.Size,
                    MonthlyRent = u.MonthlyRent,
                    Description = u.Description,
                    IsOccupied = u.IsOccupied,
                    PropertyId = property.Id,
                    PropertyName = property.Name,
                    PropertyAddress = property.Address,
                    PropertyCity = property.City
                })
                .ToList();

            return Result<IEnumerable<PublicUnitListingDto>>.Success(vacantUnits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vacant units for property {PropertyId}", propertyId);
            return Result<IEnumerable<PublicUnitListingDto>>.Failure("An error occurred while retrieving vacant units");
        }
    }

    public async Task<Result<PublicUnitListingDto>> GetUnitDetailsAsync(int unitId)
    {
        try
        {
            var unit = await _unitRepository.GetUnitWithDetailsAsync(unitId);

            if (unit == null)
            {
                return Result<PublicUnitListingDto>.Failure($"Unit with ID {unitId} not found");
            }

            var dto = new PublicUnitListingDto
            {
                Id = unit.Id,
                UnitNumber = unit.UnitNumber,
                UnitType = unit.UnitType,
                Bedrooms = unit.Bedrooms,
                Bathrooms = unit.Bathrooms,
                Size = unit.Size,
                MonthlyRent = unit.MonthlyRent,
                Description = unit.Description,
                IsOccupied = unit.IsOccupied,
                PropertyId = unit.PropertyId,
                PropertyName = unit.Property?.Name,
                PropertyAddress = unit.Property?.Address,
                PropertyCity = unit.Property?.City
            };

            return Result<PublicUnitListingDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unit details for unit {UnitId}", unitId);
            return Result<PublicUnitListingDto>.Failure("An error occurred while retrieving unit details");
        }
    }
}
