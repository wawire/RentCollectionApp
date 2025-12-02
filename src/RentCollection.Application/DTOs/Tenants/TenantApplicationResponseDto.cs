using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.Tenants;

/// <summary>
/// DTO for landlord to review tenant applications
/// </summary>
public class TenantApplicationResponseDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? IdNumber { get; set; }
    public int UnitId { get; set; }
    public string? UnitNumber { get; set; }
    public string? PropertyName { get; set; }
    public DateTime LeaseStartDate { get; set; }
    public DateTime? LeaseEndDate { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal? SecurityDeposit { get; set; }
    public TenantStatus Status { get; set; }
    public DateTime? ApplicationDate { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string? ApplicationNotes { get; set; }
    public DateTime CreatedAt { get; set; }
}
