namespace RentCollection.Application.DTOs.Properties;

public class PropertyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int TotalUnits { get; set; }
    public int OccupiedUnits { get; set; }
    public int VacantUnits { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int OrganizationId { get; set; }

    // Payment Status Information
    public int UnitsPaid { get; set; }
    public int UnitsOverdue { get; set; }
    public int UnitsPending { get; set; }
    public decimal TotalExpectedRent { get; set; }
    public decimal TotalCollectedRent { get; set; }
    public decimal CollectionRate { get; set; } // Percentage of rent collected
}
