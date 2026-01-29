using RentCollection.Domain.Enums;

namespace RentCollection.Domain.Entities;

/// <summary>
/// Tracks M-Pesa transactions (C2B payments and B2C disbursements) for audit and reconciliation
/// </summary>
public class MPesaTransaction
{
    public int Id { get; set; }

    // Transaction Type
    public MPesaTransactionType TransactionType { get; set; }

    // STK Push Request Details (C2B)
    public string MerchantRequestID { get; set; } = string.Empty;
    public string CheckoutRequestID { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string AccountReference { get; set; } = string.Empty;
    public string TransactionDesc { get; set; } = string.Empty;

    // B2C Request Details
    public string? OriginatorConversationID { get; set; }
    public string? ConversationID { get; set; }
    public string? InitiatorName { get; set; }
    public string? CommandID { get; set; } // BusinessPayment, SalaryPayment, PromotionPayment

    // Related Entities
    public int? TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public int? PaymentId { get; set; }
    public Payment? Payment { get; set; }

    public int? MoveOutInspectionId { get; set; }
    public MoveOutInspection? MoveOutInspection { get; set; }

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
