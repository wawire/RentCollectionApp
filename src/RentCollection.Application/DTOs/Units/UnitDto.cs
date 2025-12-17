using RentCollection.Application.DTOs.Amenities;
using RentCollection.Application.DTOs.Images;
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
    public string? ImageUrl { get; set; } // Deprecated - use Images list instead
    public List<ImageDto> Images { get; set; } = new();
    public List<AmenityDto> Amenities { get; set; } = new();
    public bool IsOccupied { get; set; }
    public bool IsActive { get; set; }
    public RentalType RentalType { get; set; }
    public string? CurrentTenantName { get; set; }
    public DateTime CreatedAt { get; set; }

    // Payment Status Information
    public DateTime? LastPaymentDate { get; set; }
    public decimal? LastPaymentAmount { get; set; }
    public string? PaymentStatus { get; set; } // "Paid", "Overdue", "Pending", "NoTenant"
    public int? DaysOverdue { get; set; }
    public int? CurrentTenantId { get; set; }
}
