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
    public string? ImageUrl { get; set; }
    public bool IsOccupied { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public RentalType RentalType { get; set; } = RentalType.Rent;

    /// <summary>
    /// Payment account number for this unit (used in M-Pesa Paybill as Account Number)
    /// Example: "A101", "SUN-A101", etc.
    /// This is the account number tenants use when paying via M-Pesa Paybill
    /// </summary>
    public string? PaymentAccountNumber { get; set; }

    // Navigation properties
    public Property Property { get; set; } = null!;
    public ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
