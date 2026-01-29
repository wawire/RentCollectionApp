namespace RentCollection.Application.DTOs.Utilities;

public class MeterReadingDto
{
    public int Id { get; set; }
    public int UtilityConfigId { get; set; }
    public int UnitId { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    public string UtilityName { get; set; } = string.Empty;
    public DateTime ReadingDate { get; set; }
    public decimal ReadingValue { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Notes { get; set; }
}

public class CreateMeterReadingDto
{
    public int UtilityConfigId { get; set; }
    public int UnitId { get; set; }
    public DateTime ReadingDate { get; set; }
    public decimal ReadingValue { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Notes { get; set; }
}
