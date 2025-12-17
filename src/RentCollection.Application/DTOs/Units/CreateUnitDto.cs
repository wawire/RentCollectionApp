using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.Units;

public class CreateUnitDto
{
    public string UnitNumber { get; set; } = string.Empty;
    public int PropertyId { get; set; }
    public decimal MonthlyRent { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public decimal? SquareFeet { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public RentalType RentalType { get; set; } = RentalType.Rent;
}
