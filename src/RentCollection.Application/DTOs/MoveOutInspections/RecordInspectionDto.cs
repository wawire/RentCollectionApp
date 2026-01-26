namespace RentCollection.Application.DTOs.MoveOutInspections;

public class RecordInspectionDto
{
    public string OverallCondition { get; set; } = string.Empty; // Excellent, Good, Fair, Poor
    public string GeneralNotes { get; set; } = string.Empty;

    public decimal UnpaidRent { get; set; }
    public decimal UnpaidUtilities { get; set; }
    public decimal OtherCharges { get; set; }

    public List<RecordInspectionItemDto> InspectionItems { get; set; } = new();
}

public class RecordInspectionItemDto
{
    public string Category { get; set; } = string.Empty; // Maps to InspectionCategory enum
    public string ItemName { get; set; } = string.Empty;
    public string MoveInCondition { get; set; } = string.Empty;
    public string MoveOutCondition { get; set; } = string.Empty;
    public bool IsDamaged { get; set; }
    public string? DamageDescription { get; set; }
    public decimal EstimatedRepairCost { get; set; }
    public string? Notes { get; set; }
}
