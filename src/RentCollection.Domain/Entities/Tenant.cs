using RentCollection.Domain.Common;

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
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation properties
    public Unit Unit { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
