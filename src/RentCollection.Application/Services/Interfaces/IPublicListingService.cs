using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Public;

namespace RentCollection.Application.Services.Interfaces;

/// <summary>
/// Service for public property and unit listings (no authentication required)
/// </summary>
public interface IPublicListingService
{
    /// <summary>
    /// Get all properties with vacant units
    /// </summary>
    Task<Result<IEnumerable<PublicPropertyListingDto>>> GetPublicPropertiesAsync();

    /// <summary>
    /// Get specific property details with available units
    /// </summary>
    Task<Result<PublicPropertyListingDto>> GetPublicPropertyByIdAsync(int propertyId);

    /// <summary>
    /// Get all vacant units across all properties
    /// </summary>
    Task<Result<IEnumerable<PublicUnitListingDto>>> GetVacantUnitsAsync();

    /// <summary>
    /// Get vacant units for a specific property
    /// </summary>
    Task<Result<IEnumerable<PublicUnitListingDto>>> GetVacantUnitsByPropertyAsync(int propertyId);

    /// <summary>
    /// Get details of a specific unit
    /// </summary>
    Task<Result<PublicUnitListingDto>> GetUnitDetailsAsync(int unitId);
}
