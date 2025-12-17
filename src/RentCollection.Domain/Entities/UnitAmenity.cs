namespace RentCollection.Domain.Entities;

public class UnitAmenity
{
    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;

    public int AmenityId { get; set; }
    public Amenity Amenity { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
