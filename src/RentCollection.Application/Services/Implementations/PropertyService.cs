using AutoMapper;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Common.Exceptions;
using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Properties;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;

namespace RentCollection.Application.Services.Implementations;

public class PropertyService : IPropertyService
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PropertyService> _logger;
    private readonly ICurrentUserService _currentUserService;

    public PropertyService(
        IPropertyRepository propertyRepository,
        IMapper mapper,
        ILogger<PropertyService> logger,
        ICurrentUserService currentUserService)
    {
        _propertyRepository = propertyRepository;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IEnumerable<PropertyDto>>> GetAllPropertiesAsync()
    {
        try
        {
            var properties = await _propertyRepository.GetPropertiesWithUnitsAsync();

            // Filter based on user role
            if (!_currentUserService.IsSystemAdmin)
            {
                // Landlords, Caretakers, and Accountants only see properties they have access to
                var landlordId = _currentUserService.IsLandlord ? _currentUserService.UserIdInt : _currentUserService.LandlordIdInt;
                if (landlordId.HasValue)
                {
                    properties = properties.Where(p => p.LandlordId == landlordId.Value).ToList();
                }
            }

            var propertyDtos = _mapper.Map<IEnumerable<PropertyDto>>(properties);

            return Result<IEnumerable<PropertyDto>>.Success(propertyDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all properties");
            return Result<IEnumerable<PropertyDto>>.Failure("An error occurred while retrieving properties");
        }
    }

    public async Task<Result<PropertyDto>> GetPropertyByIdAsync(int id)
    {
        try
        {
            var property = await _propertyRepository.GetPropertyWithUnitsAsync(id);

            if (property == null)
            {
                return Result<PropertyDto>.Failure($"Property with ID {id} not found");
            }

            // Check access permission
            if (!_currentUserService.IsSystemAdmin)
            {
                var landlordId = _currentUserService.IsLandlord ? _currentUserService.UserIdInt : _currentUserService.LandlordIdInt;
                if (landlordId.HasValue)
                {
                    if (property.LandlordId != landlordId.Value)
                    {
                        return Result<PropertyDto>.Failure("You do not have permission to access this property");
                    }
                }
            }

            var propertyDto = _mapper.Map<PropertyDto>(property);
            return Result<PropertyDto>.Success(propertyDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving property with ID {PropertyId}", id);
            return Result<PropertyDto>.Failure("An error occurred while retrieving the property");
        }
    }

    public async Task<Result<PropertyDto>> CreatePropertyAsync(CreatePropertyDto createDto)
    {
        try
        {
            var property = _mapper.Map<Property>(createDto);
            property.CreatedAt = DateTime.UtcNow;

            // Set LandlordId based on current user
            if (_currentUserService.IsLandlord)
            {
                property.LandlordId = _currentUserService.UserIdInt;
            }
            else if (_currentUserService.IsCaretaker || _currentUserService.IsAccountant)
            {
                property.LandlordId = _currentUserService.LandlordIdInt;
            }
            // SystemAdmin can create properties for any landlord (would need additional logic/DTO field)

            var createdProperty = await _propertyRepository.AddAsync(property);
            var propertyDto = _mapper.Map<PropertyDto>(property);

            _logger.LogInformation("Property created successfully: {PropertyName} for Landlord {LandlordId}", createdProperty.Name, property.LandlordId);
            return Result<PropertyDto>.Success(propertyDto, "Property created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating property");
            return Result<PropertyDto>.Failure("An error occurred while creating the property");
        }
    }

    public async Task<Result<PropertyDto>> UpdatePropertyAsync(int id, UpdatePropertyDto updateDto)
    {
        try
        {
            var existingProperty = await _propertyRepository.GetByIdAsync(id);

            if (existingProperty == null)
            {
                return Result<PropertyDto>.Failure($"Property with ID {id} not found");
            }

            // Check access permission
            if (!_currentUserService.IsSystemAdmin)
            {
                var landlordId = _currentUserService.IsLandlord ? _currentUserService.UserIdInt : _currentUserService.LandlordIdInt;

                if (landlordId.HasValue)
                {
                    if (existingProperty.LandlordId != landlordId.Value)
                    {
                        return Result<PropertyDto>.Failure("You do not have permission to update this property");
                    }
                }

                // Accountants have read-only access
                if (_currentUserService.IsAccountant)
                {
                    return Result<PropertyDto>.Failure("Accountants do not have permission to modify properties");
                }
            }

            _mapper.Map(updateDto, existingProperty);
            existingProperty.UpdatedAt = DateTime.UtcNow;

            await _propertyRepository.UpdateAsync(existingProperty);

            var propertyDto = _mapper.Map<PropertyDto>(existingProperty);

            _logger.LogInformation("Property updated successfully: {PropertyId}", id);
            return Result<PropertyDto>.Success(propertyDto, "Property updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating property with ID {PropertyId}", id);
            return Result<PropertyDto>.Failure("An error occurred while updating the property");
        }
    }

    public async Task<Result> DeletePropertyAsync(int id)
    {
        try
        {
            var property = await _propertyRepository.GetPropertyWithUnitsAsync(id);

            if (property == null)
            {
                return Result.Failure($"Property with ID {id} not found");
            }

            // Check access permission - Only SystemAdmin and Landlords can delete
            if (!_currentUserService.IsSystemAdmin && !_currentUserService.IsLandlord)
            {
                return Result.Failure("You do not have permission to delete properties");
            }

            if (!_currentUserService.IsSystemAdmin)
            {
                var landlordId = _currentUserService.UserIdInt; // Must be landlord at this point

                if (landlordId.HasValue)
                {
                    if (property.LandlordId != landlordId.Value)
                    {
                        return Result.Failure("You do not have permission to delete this property");
                    }
                }
            }

            // Check if property has units
            if (property.Units.Any())
            {
                return Result.Failure("Cannot delete property with existing units. Please delete all units first.");
            }

            await _propertyRepository.DeleteAsync(property);

            _logger.LogInformation("Property deleted successfully: {PropertyId}", id);
            return Result.Success("Property deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting property with ID {PropertyId}", id);
            return Result.Failure("An error occurred while deleting the property");
        }
    }

    public async Task<Result<PaginatedList<PropertyDto>>> GetPropertiesPaginatedAsync(int pageNumber, int pageSize)
    {
        try
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Max page size

            var allProperties = await _propertyRepository.GetPropertiesWithUnitsAsync();

            // Filter based on user role
            if (!_currentUserService.IsSystemAdmin)
            {
                var landlordId = _currentUserService.IsLandlord ? _currentUserService.UserIdInt : _currentUserService.LandlordIdInt;
                if (landlordId.HasValue)
                {
                    allProperties = allProperties.Where(p => p.LandlordId == landlordId.Value).ToList();
                }
            }

            var totalCount = allProperties.Count();

            var paginatedProperties = allProperties
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var propertyDtos = _mapper.Map<List<PropertyDto>>(paginatedProperties);
            var paginatedList = new PaginatedList<PropertyDto>(propertyDtos, totalCount, pageNumber, pageSize);

            return Result<PaginatedList<PropertyDto>>.Success(paginatedList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paginated properties");
            return Result<PaginatedList<PropertyDto>>.Failure("An error occurred while retrieving properties");
        }
    }
}
