using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Interfaces
{
    public interface IMaintenanceRequestRepository : IRepository<MaintenanceRequest>
    {
        Task<List<MaintenanceRequest>> GetByTenantIdAsync(int tenantId);
        Task<List<MaintenanceRequest>> GetByPropertyIdAsync(int propertyId);
        Task<List<MaintenanceRequest>> GetByStatusAsync(MaintenanceRequestStatus status);
        Task<List<MaintenanceRequest>> GetByAssignedUserIdAsync(int userId);
        Task<MaintenanceRequest?> GetWithDetailsAsync(int id);
    }
}
