using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.MoveOutInspections;

public class InspectionItemDto
{
    public int Id { get; set; }
    public int MoveOutInspectionId { get; set; }

    public InspectionCategory Category { get; set; }
    public string CategoryDisplay { get; set; } = string.Empty;

    public string ItemName { get; set; } = string.Empty;
    public string MoveInCondition { get; set; } = string.Empty;
    public string MoveOutCondition { get; set; } = string.Empty;

    public bool IsDamaged { get; set; }
    public string? DamageDescription { get; set; }
    public decimal EstimatedRepairCost { get; set; }

    public string? Notes { get; set; }

    public List<InspectionPhotoDto> Photos { get; set; } = new();
}
