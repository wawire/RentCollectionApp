namespace RentCollection.Application.DTOs.Public;

/// <summary>
/// Public DTO for property listings (no authentication required)
/// </summary>
public class PublicPropertyListingDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public int TotalUnits { get; set; }
    public int VacantUnits { get; set; }
    public List<PublicUnitListingDto> AvailableUnits { get; set; } = new();
}
