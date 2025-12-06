using RentCollection.Domain.Entities;

namespace RentCollection.Application.Interfaces;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, int entityId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
