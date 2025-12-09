using RentCollection.Domain.Entities;

namespace RentCollection.Application.Interfaces
{
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        Task<List<AuditLog>> GetByUserIdAsync(int userId, int skip = 0, int take = 50);
        Task<List<AuditLog>> GetByEntityAsync(string entityType, int entityId);
    }
}
