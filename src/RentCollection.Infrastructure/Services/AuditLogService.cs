using RentCollection.Application.Common;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;

namespace RentCollection.Infrastructure.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ICurrentUserService _currentUserService;

        public AuditLogService(IAuditLogRepository auditLogRepository, ICurrentUserService currentUserService)
        {
            _auditLogRepository = auditLogRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ServiceResult<bool>> LogActionAsync(string action, string entityType, int entityId, string? details = null)
        {
            try
            {
                var userId = _currentUserService.UserIdInt ?? 0;

                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    Details = details,
                    CreatedAt = DateTime.UtcNow
                };

                await _auditLogRepository.AddAsync(auditLog);
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"Failed to log action: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<AuditLog>>> GetUserAuditLogsAsync(int userId, int skip = 0, int take = 50)
        {
            try
            {
                var logs = await _auditLogRepository.GetByUserIdAsync(userId, skip, take);
                return ServiceResult<List<AuditLog>>.Success(logs);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<AuditLog>>.Failure($"Failed to get user audit logs: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<AuditLog>>> GetEntityAuditLogsAsync(string entityType, int entityId)
        {
            try
            {
                var logs = await _auditLogRepository.GetByEntityAsync(entityType, entityId);
                return ServiceResult<List<AuditLog>>.Success(logs);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<AuditLog>>.Failure($"Failed to get entity audit logs: {ex.Message}");
            }
        }

        public async Task LogUserCreatedAsync(int userId, string email, string role)
        {
            await LogActionAsync("User.Created", "User", userId, $"User created: {email} ({role})");
        }

        public async Task LogUserStatusChangedAsync(int userId, string oldStatus, string newStatus)
        {
            await LogActionAsync("User.StatusChanged", "User", userId, $"Status changed from {oldStatus} to {newStatus}");
        }

        public async Task LogUserDeletedAsync(int userId, string email)
        {
            await LogActionAsync("User.Deleted", "User", userId, $"User deleted: {email}");
        }
    }
}
