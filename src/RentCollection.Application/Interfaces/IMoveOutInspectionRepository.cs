using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Interfaces;

public interface IMoveOutInspectionRepository : IRepository<MoveOutInspection>
{
    Task<MoveOutInspection?> GetByIdWithDetailsAsync(int id);
    Task<List<MoveOutInspection>> GetByTenantIdAsync(int tenantId);
    Task<List<MoveOutInspection>> GetByUnitIdAsync(int unitId);
    Task<List<MoveOutInspection>> GetByPropertyIdAsync(int propertyId);
    Task<List<MoveOutInspection>> GetByPropertyIdAsync(int propertyId, int landlordUserId);
    Task<List<MoveOutInspection>> GetByStatusAsync(MoveOutInspectionStatus status);
    Task<List<MoveOutInspection>> GetByStatusAsync(MoveOutInspectionStatus status, int landlordUserId);
    Task<List<MoveOutInspection>> GetPendingInspectionsAsync();
    Task<List<MoveOutInspection>> GetPendingInspectionsAsync(int landlordUserId);
    Task<MoveOutInspection?> GetLatestByTenantIdAsync(int tenantId);
    Task<bool> HasPendingInspectionAsync(int tenantId);
}
