namespace RentCollection.Domain.Entities;

public class PropertyAmenity
{
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;

    public int AmenityId { get; set; }
    public Amenity Amenity { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
