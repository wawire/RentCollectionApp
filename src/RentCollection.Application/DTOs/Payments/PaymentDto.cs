using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.Payments;

public class PaymentDto
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string UnitNumber { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }
    public string? Notes { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime CreatedAt { get; set; }
}
