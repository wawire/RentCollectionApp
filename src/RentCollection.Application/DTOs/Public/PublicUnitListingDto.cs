namespace RentCollection.Application.DTOs.Public;

/// <summary>
/// Public DTO for unit listings (no authentication required)
/// </summary>
public class PublicUnitListingDto
{
    public int Id { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    public string? UnitType { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public decimal? Size { get; set; }
    public decimal MonthlyRent { get; set; }
    public string? Description { get; set; }
    public bool IsOccupied { get; set; }
    public int PropertyId { get; set; }
    public string? PropertyName { get; set; }
    public string? PropertyAddress { get; set; }
    public string? PropertyCity { get; set; }
}
