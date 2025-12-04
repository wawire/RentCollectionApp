using System.ComponentModel.DataAnnotations;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.Payments;

/// <summary>
/// DTO for tenants to record a payment they've made
/// </summary>
public class TenantRecordPaymentDto
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// M-Pesa confirmation code or bank transaction reference
    /// </summary>
    [Required(ErrorMessage = "Transaction reference is required")]
    public string TransactionReference { get; set; } = string.Empty;

    /// <summary>
    /// M-Pesa phone number used for payment (required for M-Pesa payments)
    /// </summary>
    public string? MPesaPhoneNumber { get; set; }

    /// <summary>
    /// Optional notes about the payment
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Payment period start date
    /// </summary>
    [Required]
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// Payment period end date
    /// </summary>
    [Required]
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Payment proof file (screenshot, receipt, etc.)
    /// </summary>
    public string? PaymentProofUrl { get; set; }
}
