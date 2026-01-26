using RentCollection.Domain.Common;
using RentCollection.Domain.Enums;

namespace RentCollection.Domain.Entities;

public class UtilityConfig : BaseEntity
{
    public int UtilityTypeId { get; set; }
    public UtilityType UtilityType { get; set; } = null!;

    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;

    public int? UnitId { get; set; }
    public Unit? Unit { get; set; }

    public UtilityBillingMode BillingMode { get; set; } = UtilityBillingMode.Fixed;
    public decimal? FixedAmount { get; set; }
    public decimal? Rate { get; set; }
    public decimal? SharedAmount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow.Date;
    public DateTime? EffectiveTo { get; set; }
    public string? Notes { get; set; }

    public ICollection<MeterReading> MeterReadings { get; set; } = new List<MeterReading>();
}
