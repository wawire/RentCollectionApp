namespace RentCollection.Application.Services.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(string action, string entityType, int? entityId, string? details = null);
    Task LogUserCreatedAsync(int createdUserId, string createdUserEmail, string createdUserRole);
    Task LogUserDeletedAsync(int deletedUserId, string deletedUserEmail);
    Task LogUserStatusChangedAsync(int userId, string oldStatus, string newStatus);
    Task LogPaymentConfirmedAsync(int paymentId, int tenantId, decimal amount);
    Task LogPaymentRejectedAsync(int paymentId, int tenantId, decimal amount, string? reason);
}
