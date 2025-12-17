namespace RentCollection.Application.DTOs.Amenities;

public class AmenityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IconName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
