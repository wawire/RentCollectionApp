using RentCollection.Domain.Common;
using RentCollection.Domain.Enums;

namespace RentCollection.Domain.Entities;

public class SmsLog : BaseEntity
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public SmsStatus Status { get; set; } = SmsStatus.Pending;
    public DateTime SentAt { get; set; }
    public string? ExternalId { get; set; }
    public string? ErrorMessage { get; set; }
    public int? TenantId { get; set; }
}
