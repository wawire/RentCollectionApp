using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.Payments;

public class CreatePaymentDto
{
    public int TenantId { get; set; }
    public int? UnitId { get; set; }
    public int? LandlordAccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public DateTime? DueDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionReference { get; set; }
    public string? Notes { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}
