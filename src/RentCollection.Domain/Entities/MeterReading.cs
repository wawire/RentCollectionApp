using RentCollection.Domain.Common;

namespace RentCollection.Domain.Entities;

public class MeterReading : BaseEntity
{
    public int UtilityConfigId { get; set; }
    public UtilityConfig UtilityConfig { get; set; } = null!;

    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;

    public DateTime ReadingDate { get; set; }
    public decimal ReadingValue { get; set; }

    public string? PhotoUrl { get; set; }
    public string? Notes { get; set; }
    public int? RecordedByUserId { get; set; }
    public User? RecordedByUser { get; set; }
}
