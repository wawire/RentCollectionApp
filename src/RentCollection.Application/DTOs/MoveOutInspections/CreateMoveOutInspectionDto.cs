namespace RentCollection.Application.DTOs.MoveOutInspections;

public class CreateMoveOutInspectionDto
{
    public int TenantId { get; set; }
    public DateTime MoveOutDate { get; set; }
    public DateTime InspectionDate { get; set; }
    public string? Notes { get; set; }
}
