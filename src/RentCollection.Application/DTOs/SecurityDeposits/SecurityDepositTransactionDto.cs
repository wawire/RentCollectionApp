using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.SecurityDeposits;

public class SecurityDepositTransactionDto
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string UnitNumber { get; set; } = string.Empty;

    public decimal Amount { get; set; }
    public SecurityDepositTransactionType TransactionType { get; set; }
    public string TransactionTypeDisplay { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime TransactionDate { get; set; }

    public int? RelatedPaymentId { get; set; }
    public int? RelatedMaintenanceRequestId { get; set; }

    public string? ReceiptUrl { get; set; }
    public string? Notes { get; set; }

    public int CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
