using RentCollection.Domain.Common;
using RentCollection.Domain.Enums;

namespace RentCollection.Domain.Entities;

public class Tenant : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? IdNumber { get; set; }
    public int UnitId { get; set; }
    public DateTime LeaseStartDate { get; set; }
    public DateTime? LeaseEndDate { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal? SecurityDeposit { get; set; }

    /// <summary>
    /// Legacy field - use Status instead. Kept for backward compatibility.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Current status in tenant lifecycle (Prospective, Active, Inactive)
    /// </summary>
    public TenantStatus Status { get; set; } = TenantStatus.Active;

    /// <summary>
    /// Date when tenant submitted application (for Prospective tenants)
    /// </summary>
    public DateTime? ApplicationDate { get; set; }

    /// <summary>
    /// Date when landlord approved the application
    /// </summary>
    public DateTime? ApprovalDate { get; set; }

    /// <summary>
    /// Notes from tenant during application or landlord during review
    /// </summary>
    public string? ApplicationNotes { get; set; }

    /// <summary>
    /// User account ID after tenant is approved and given login access
    /// </summary>
    public int? UserId { get; set; }

    public string? Notes { get; set; }

    // Navigation properties
    public Unit Unit { get; set; } = null!;
    public User? User { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
