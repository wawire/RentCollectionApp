using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;

namespace RentCollection.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(
        IAuditLogRepository auditLogRepository,
        ICurrentUserService currentUserService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditLogService> logger)
    {
        _auditLogRepository = auditLogRepository;
        _currentUserService = currentUserService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogAsync(string action, string entityType, int? entityId, string? details = null)
    {
        try
        {
            var userId = _currentUserService.UserIdInt ?? 0;
            if (userId == 0)
            {
                _logger.LogWarning("Attempted to log audit without authenticated user. Action: {Action}", action);
                return;
            }

            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedAt = DateTime.UtcNow
            };

            await _auditLogRepository.AddAsync(auditLog);

            _logger.LogInformation("Audit log created: {Action} on {EntityType}#{EntityId} by User#{UserId}",
                action, entityType, entityId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create audit log for action: {Action}", action);
        }
    }

    public async Task LogUserCreatedAsync(int createdUserId, string createdUserEmail, string createdUserRole)
    {
        var details = JsonSerializer.Serialize(new
        {
            email = createdUserEmail,
            role = createdUserRole,
            createdAt = DateTime.UtcNow
        });

        await LogAsync("UserCreated", "User", createdUserId, details);
    }

    public async Task LogUserDeletedAsync(int deletedUserId, string deletedUserEmail)
    {
        var details = JsonSerializer.Serialize(new
        {
            email = deletedUserEmail,
            deletedAt = DateTime.UtcNow
        });

        await LogAsync("UserDeleted", "User", deletedUserId, details);
    }

    public async Task LogUserStatusChangedAsync(int userId, string oldStatus, string newStatus)
    {
        var details = JsonSerializer.Serialize(new
        {
            oldStatus,
            newStatus,
            changedAt = DateTime.UtcNow
        });

        await LogAsync("UserStatusChanged", "User", userId, details);
    }

    public async Task LogPaymentConfirmedAsync(int paymentId, int tenantId, decimal amount)
    {
        var details = JsonSerializer.Serialize(new
        {
            tenantId,
            amount,
            confirmedAt = DateTime.UtcNow
        });

        await LogAsync("PaymentConfirmed", "Payment", paymentId, details);
    }

    public async Task LogPaymentRejectedAsync(int paymentId, int tenantId, decimal amount, string? reason)
    {
        var details = JsonSerializer.Serialize(new
        {
            tenantId,
            amount,
            reason,
            rejectedAt = DateTime.UtcNow
        });

        await LogAsync("PaymentRejected", "Payment", paymentId, details);
    }
}
