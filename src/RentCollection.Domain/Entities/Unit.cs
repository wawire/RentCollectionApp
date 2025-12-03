using RentCollection.Domain.Common;
using RentCollection.Domain.Enums;

namespace RentCollection.Domain.Entities;

public class Unit : BaseEntity
{
    public string UnitNumber { get; set; } = string.Empty;
    public int PropertyId { get; set; }
    public decimal MonthlyRent { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public decimal? SquareFeet { get; set; }
    public string? Description { get; set; }
    public bool IsOccupied { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public RentalType RentalType { get; set; } = RentalType.Rent;

    // Navigation properties
    public Property Property { get; set; } = null!;
    public ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();
}
