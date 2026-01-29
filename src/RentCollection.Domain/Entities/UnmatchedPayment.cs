using RentCollection.Domain.Common;
using RentCollection.Domain.Enums;

namespace RentCollection.Domain.Entities;

public class UnmatchedPayment : BaseEntity
{
    public string TransactionReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string AccountReference { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? BusinessShortCode { get; set; }
    public string? CorrelationId { get; set; }
    public string RawPayload { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int? LandlordId { get; set; }
    public int? PropertyId { get; set; }
    public int? ResolvedPaymentId { get; set; }
    public int? ResolvedByUserId { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    public UnmatchedPaymentStatus Status { get; set; } = UnmatchedPaymentStatus.Pending;
}
