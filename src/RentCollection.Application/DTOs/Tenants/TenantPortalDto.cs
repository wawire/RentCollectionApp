using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.Tenants;

/// <summary>
/// DTO for tenant's own portal view with their lease and unit details
/// </summary>
public class TenantPortalDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? IdNumber { get; set; }

    // Lease Information
    public DateTime LeaseStartDate { get; set; }
    public DateTime? LeaseEndDate { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal? SecurityDeposit { get; set; }
    public TenantStatus Status { get; set; }

    // Unit Information
    public int UnitId { get; set; }
    public string? UnitNumber { get; set; }
    public string? UnitType { get; set; }
    public decimal? UnitSize { get; set; }

    // Property Information
    public int PropertyId { get; set; }
    public string? PropertyName { get; set; }
    public string? PropertyAddress { get; set; }
    public string? PropertyCity { get; set; }

    // Payment Summary
    public decimal TotalPaid { get; set; }
    public decimal OutstandingBalance { get; set; }
    public DateTime? LastPaymentDate { get; set; }
}
