using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.Units;

public class UnitDto
{
    public int Id { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    public int PropertyId { get; set; }
    public string PropertyName { get; set; } = string.Empty;
    public decimal MonthlyRent { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public decimal? SquareFeet { get; set; }
    public string? Description { get; set; }
    public bool IsOccupied { get; set; }
    public bool IsActive { get; set; }
    public RentalType RentalType { get; set; }
    public string? CurrentTenantName { get; set; }
    public DateTime CreatedAt { get; set; }
}
