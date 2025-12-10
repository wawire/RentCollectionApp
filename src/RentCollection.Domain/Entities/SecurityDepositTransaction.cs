using RentCollection.Domain.Enums;

namespace RentCollection.Domain.Entities;

public class SecurityDepositTransaction
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public decimal Amount { get; set; }
    public SecurityDepositTransactionType TransactionType { get; set; }
    public string? Reason { get; set; }
    public DateTime TransactionDate { get; set; }

    public int? RelatedPaymentId { get; set; }
    public Payment? RelatedPayment { get; set; }

    public int? RelatedMaintenanceRequestId { get; set; }
    public MaintenanceRequest? RelatedMaintenanceRequest { get; set; }

    public string? ReceiptUrl { get; set; }
    public string? Notes { get; set; }

    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
