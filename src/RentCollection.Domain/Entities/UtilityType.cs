using RentCollection.Domain.Common;
using RentCollection.Domain.Enums;

namespace RentCollection.Domain.Entities;

public class UtilityType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public UtilityBillingMode BillingMode { get; set; } = UtilityBillingMode.Fixed;
    public string? UnitOfMeasure { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }

    public ICollection<UtilityConfig> UtilityConfigs { get; set; } = new List<UtilityConfig>();
}
