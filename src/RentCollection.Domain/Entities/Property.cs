using RentCollection.Domain.Common;

namespace RentCollection.Domain.Entities;

public class Property : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int TotalUnits { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<Unit> Units { get; set; } = new List<Unit>();
}
