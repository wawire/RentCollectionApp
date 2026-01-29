using System;

namespace RentCollection.Application.DTOs.Payments;

public class ResolveUnmatchedPaymentDto
{
    public int TenantId { get; set; }
    public int? LandlordAccountId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? Notes { get; set; }
}
