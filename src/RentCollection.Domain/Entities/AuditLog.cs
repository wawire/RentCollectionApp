using RentCollection.Domain.Enums;

namespace RentCollection.Domain.Entities;

/// <summary>
/// Audit log entity for tracking sensitive operations
/// </summary>
public class AuditLog
{
    public int Id { get; set; }

    /// <summary>
    /// User who performed the action
    /// </summary>
    public int UserId { get; set; }
    public User? User { get; set; }

    /// <summary>
    /// Action performed (e.g., "UserCreated", "UserDeleted", "PaymentConfirmed")
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Entity type affected (e.g., "User", "Payment", "Property")
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the entity affected
    /// </summary>
    public int? EntityId { get; set; }

    /// <summary>
    /// Additional details in JSON format
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// IP address of the user
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent (browser/client info)
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Timestamp of the action
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
