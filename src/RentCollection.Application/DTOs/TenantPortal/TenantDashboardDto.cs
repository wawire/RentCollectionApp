namespace RentCollection.Application.DTOs.TenantPortal;

/// <summary>
/// DTO for tenant dashboard with key metrics and information
/// </summary>
public class TenantDashboardDto
{
    /// <summary>
    /// Tenant information
    /// </summary>
    public TenantInfoDto TenantInfo { get; set; } = null!;

    /// <summary>
    /// Current balance (total amount owed)
    /// </summary>
    public decimal CurrentBalance { get; set; }

    /// <summary>
    /// Next payment due date
    /// </summary>
    public DateTime? NextPaymentDueDate { get; set; }

    /// <summary>
    /// Next payment amount
    /// </summary>
    public decimal? NextPaymentAmount { get; set; }

    /// <summary>
    /// Days until next payment is due (negative if overdue)
    /// </summary>
    public int? DaysUntilDue { get; set; }

    /// <summary>
    /// Indicates if tenant has overdue payments
    /// </summary>
    public bool HasOverduePayments { get; set; }

    /// <summary>
    /// Total overdue amount
    /// </summary>
    public decimal OverdueAmount { get; set; }

    /// <summary>
    /// Number of days overdue (for oldest overdue payment)
    /// </summary>
    public int DaysOverdue { get; set; }

    /// <summary>
    /// Total payments made (all time)
    /// </summary>
    public int TotalPaymentsMade { get; set; }

    /// <summary>
    /// Total amount paid (all time)
    /// </summary>
    public decimal TotalAmountPaid { get; set; }

    /// <summary>
    /// Recent payments (last 5)
    /// </summary>
    public List<RecentPaymentDto> RecentPayments { get; set; } = new();

    /// <summary>
    /// Pending payments awaiting confirmation
    /// </summary>
    public List<RecentPaymentDto> PendingPayments { get; set; } = new();

    /// <summary>
    /// Number of documents uploaded
    /// </summary>
    public int DocumentCount { get; set; }

    /// <summary>
    /// Lease expiry date
    /// </summary>
    public DateTime? LeaseExpiryDate { get; set; }

    /// <summary>
    /// Days until lease expires
    /// </summary>
    public int? DaysUntilLeaseExpiry { get; set; }

    /// <summary>
    /// Late fee policy information
    /// </summary>
    public string LateFeePolicy { get; set; } = string.Empty;
}

/// <summary>
/// Basic tenant information for dashboard
/// </summary>
public class TenantInfoDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public string UnitNumber { get; set; } = string.Empty;
    public decimal MonthlyRent { get; set; }
    public DateTime LeaseStartDate { get; set; }
    public DateTime? LeaseEndDate { get; set; }
}

/// <summary>
/// Recent payment summary for dashboard
/// </summary>
public class RecentPaymentDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public decimal LateFeeAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime PaymentDate { get; set; }
    public DateTime DueDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }
    public bool IsLate { get; set; }
    public int DaysOverdue { get; set; }
}
