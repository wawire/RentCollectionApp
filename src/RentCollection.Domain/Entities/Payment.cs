using RentCollection.Domain.Common;
using RentCollection.Domain.Enums;

namespace RentCollection.Domain.Entities;

public class Payment : BaseEntity
{
    public int TenantId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? TransactionReference { get; set; }
    public string? Notes { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

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

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public User? ConfirmedBy { get; set; }
}
