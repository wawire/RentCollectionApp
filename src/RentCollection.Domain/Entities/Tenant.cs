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
    /// Day of the month when rent is due (1-31). Default is 5th of each month.
    /// </summary>
    public int RentDueDay { get; set; } = 5;

    /// <summary>
    /// Grace period in days before late payment penalties apply. Default is 3 days.
    /// </summary>
    public int LateFeeGracePeriodDays { get; set; } = 3;

    /// <summary>
    /// Late payment fee as a percentage of monthly rent. Default is 5% (0.05).
    /// For example, 0.05 = 5% penalty, 0.10 = 10% penalty.
    /// </summary>
    public decimal LateFeePercentage { get; set; } = 0.05m;

    /// <summary>
    /// Fixed late payment fee amount (optional, used instead of percentage if set).
    /// If both percentage and fixed amount are set, fixed amount takes precedence.
    /// </summary>
    public decimal? LateFeeFixedAmount { get; set; }

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

    public string? Notes { get; set; }

    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();

    // Navigation properties
    public Unit Unit { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
