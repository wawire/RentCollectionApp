using RentCollection.Domain.Common;

namespace RentCollection.Domain.Entities;

public class UnitImage : BaseEntity
{
    public int UnitId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; } = 0;
    public bool IsPrimary { get; set; } = false;

    // Navigation property
    public Unit Unit { get; set; } = null!;
}
