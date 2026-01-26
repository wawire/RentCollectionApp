using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.MoveOutInspections;

public class InspectionPhotoDto
{
    public int Id { get; set; }
    public int MoveOutInspectionId { get; set; }
    public int? InspectionItemId { get; set; }

    public string PhotoUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }

    public PhotoType PhotoType { get; set; }
    public string PhotoTypeDisplay { get; set; } = string.Empty;

    public DateTime TakenAt { get; set; }
}
