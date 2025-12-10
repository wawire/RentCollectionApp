using RentCollection.Domain.Enums;

namespace RentCollection.Domain.Entities;

/// <summary>
/// Tracks M-Pesa STK Push transactions for audit and reconciliation
/// </summary>
public class MPesaTransaction
{
    public int Id { get; set; }

    // STK Push Request Details
    public string MerchantRequestID { get; set; } = string.Empty;
    public string CheckoutRequestID { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string AccountReference { get; set; } = string.Empty;
    public string TransactionDesc { get; set; } = string.Empty;

    // Related Entities
    public int? TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public int? PaymentId { get; set; }
    public Payment? Payment { get; set; }

    // Transaction Status
    public MPesaTransactionStatus Status { get; set; }
    public int ResultCode { get; set; }
    public string ResultDesc { get; set; } = string.Empty;

    // M-Pesa Response Details
    public string? MPesaReceiptNumber { get; set; }
    public DateTime? TransactionDate { get; set; }

    // Request/Response Logs (for debugging)
    public string? RequestJson { get; set; }
    public string? ResponseJson { get; set; }
    public string? CallbackJson { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CallbackReceivedAt { get; set; }
}
