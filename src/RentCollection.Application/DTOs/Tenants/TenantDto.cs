namespace RentCollection.Application.DTOs.Tenants;

public class TenantDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? IdNumber { get; set; }
    public int UnitId { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public DateTime LeaseStartDate { get; set; }
    public DateTime? LeaseEndDate { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal? SecurityDeposit { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    // Payment Status Information
    public DateTime? LastPaymentDate { get; set; }
    public decimal? LastPaymentAmount { get; set; }
    public string? PaymentStatus { get; set; } // "Paid", "Overdue", "Pending", "NoPayment"
    public int? DaysOverdue { get; set; }
    public int RentDueDay { get; set; }
}
