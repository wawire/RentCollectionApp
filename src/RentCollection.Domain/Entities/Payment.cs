using RentCollection.Domain.Common;
using RentCollection.Domain.Enums;

namespace RentCollection.Domain.Entities;

public class Payment : BaseEntity
{
    public int TenantId { get; set; }

    /// <summary>
    /// Unit this payment is for
    /// </summary>
    public int UnitId { get; set; }

    /// <summary>
    /// Landlord payment account this payment was sent to
    /// </summary>
    public int LandlordAccountId { get; set; }

    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }

    /// <summary>
    /// Due date for this payment. If payment is made after this date, it's considered late.
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Late payment penalty/fee amount applied to this payment
    /// </summary>
    public decimal LateFeeAmount { get; set; } = 0;

    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? TransactionReference { get; set; }
    public string? Notes { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// M-Pesa Paybill account number used (e.g., "A101")
    /// </summary>
    public string? PaybillAccountNumber { get; set; }

    /// <summary>
    /// Tenant's M-Pesa phone number that made the payment
    /// </summary>
    public string? MPesaPhoneNumber { get; set; }

    /// <summary>
    /// URL/path to payment proof (M-Pesa screenshot, bank receipt, etc.)
    /// </summary>
    public string? PaymentProofUrl { get; set; }

    /// <summary>
    /// Date when payment was confirmed by landlord/caretaker
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>
    /// User ID of landlord/caretaker who confirmed the payment
    /// </summary>
    public int? ConfirmedByUserId { get; set; }

    // Computed properties for late payment tracking
    /// <summary>
    /// Indicates if the payment was made after the due date
    /// </summary>
    public bool IsLate => PaymentDate > DueDate;

    /// <summary>
    /// Number of days the payment is overdue (0 if not late)
    /// </summary>
    public int DaysOverdue => IsLate ? (PaymentDate.Date - DueDate.Date).Days : 0;

    /// <summary>
    /// Indicates if payment is currently pending and past due date
    /// </summary>
    public bool IsPendingAndOverdue => Status == PaymentStatus.Pending && DateTime.UtcNow.Date > DueDate.Date;

    /// <summary>
    /// Days currently overdue for pending payments (0 if not applicable)
    /// </summary>
    public int CurrentDaysOverdue => IsPendingAndOverdue ? (DateTime.UtcNow.Date - DueDate.Date).Days : 0;

    /// <summary>
    /// Total amount including base amount and late fee
    /// </summary>
    public decimal TotalAmount => Amount + LateFeeAmount;

    /// <summary>
    /// Indicates if a late fee has been applied to this payment
    /// </summary>
    public bool HasLateFee => LateFeeAmount > 0;

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public Unit Unit { get; set; } = null!;
    public LandlordPaymentAccount LandlordAccount { get; set; } = null!;
    public User? ConfirmedBy { get; set; }
}
