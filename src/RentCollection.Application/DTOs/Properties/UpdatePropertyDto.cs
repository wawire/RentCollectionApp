namespace RentCollection.Application.DTOs.Properties;

public class UpdatePropertyDto
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int TotalUnits { get; set; }
    public bool IsActive { get; set; }
}
