using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.Payments;

public class UnmatchedPaymentDto
{
    public int Id { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string AccountReference { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? BusinessShortCode { get; set; }
    public string? CorrelationId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int? LandlordId { get; set; }
    public int? PropertyId { get; set; }
    public int? ResolvedPaymentId { get; set; }
    public int? ResolvedByUserId { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    public UnmatchedPaymentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
