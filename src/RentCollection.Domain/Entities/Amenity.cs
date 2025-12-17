using RentCollection.Domain.Common;

namespace RentCollection.Domain.Entities;

public class Amenity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string IconName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Navigation properties for many-to-many relationships
    public ICollection<PropertyAmenity> PropertyAmenities { get; set; } = new List<PropertyAmenity>();
    public ICollection<UnitAmenity> UnitAmenities { get; set; } = new List<UnitAmenity>();
}
