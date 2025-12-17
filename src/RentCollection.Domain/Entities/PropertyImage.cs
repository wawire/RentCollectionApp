using RentCollection.Domain.Common;

namespace RentCollection.Domain.Entities;

public class PropertyImage : BaseEntity
{
    public int PropertyId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; } = 0;
    public bool IsPrimary { get; set; } = false;

    // Navigation property
    public Property Property { get; set; } = null!;
}
