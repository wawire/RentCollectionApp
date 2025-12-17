using RentCollection.Domain.Common;

namespace RentCollection.Domain.Entities;

public class Property : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int TotalUnits { get; set; }
    public bool IsActive { get; set; } = true;

    // Owner of the property (Landlord User ID)
    public int? LandlordId { get; set; }
    public User? Landlord { get; set; }

    // Navigation properties
    public ICollection<Unit> Units { get; set; } = new List<Unit>();
    public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
    public ICollection<PropertyAmenity> PropertyAmenities { get; set; } = new List<PropertyAmenity>();
    public ICollection<LandlordPaymentAccount> PaymentAccounts { get; set; } = new List<LandlordPaymentAccount>();
}
