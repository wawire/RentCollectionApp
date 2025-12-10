using RentCollection.Application.Common;
using RentCollection.Domain.Entities;

namespace RentCollection.Application.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task<ServiceResult<bool>> LogActionAsync(string action, string entityType, int entityId, string? details = null);
        Task<ServiceResult<List<AuditLog>>> GetUserAuditLogsAsync(int userId, int skip = 0, int take = 50);
        Task<ServiceResult<List<AuditLog>>> GetEntityAuditLogsAsync(string entityType, int entityId);
        Task LogUserCreatedAsync(int userId, string email, string role);
        Task LogUserStatusChangedAsync(int userId, string oldStatus, string newStatus);
        Task LogUserDeletedAsync(int userId, string email);
    }
}
