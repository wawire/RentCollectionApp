using RentCollection.Application.Common;
using RentCollection.Application.DTOs.MaintenanceRequests;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Services.Interfaces
{
    public interface IMaintenanceRequestService
    {
        Task<ServiceResult<MaintenanceRequestDto>> CreateMaintenanceRequestAsync(CreateMaintenanceRequestDto dto);
        Task<ServiceResult<List<MaintenanceRequestDto>>> GetAllMaintenanceRequestsAsync();
        Task<ServiceResult<MaintenanceRequestDto>> GetMaintenanceRequestByIdAsync(int id);
        Task<ServiceResult<List<MaintenanceRequestDto>>> GetMyMaintenanceRequestsAsync();
        Task<ServiceResult<List<MaintenanceRequestDto>>> GetMaintenanceRequestsByPropertyIdAsync(int propertyId);
        Task<ServiceResult<List<MaintenanceRequestDto>>> GetMaintenanceRequestsByStatusAsync(MaintenanceRequestStatus status);
        Task<ServiceResult<List<MaintenanceRequestDto>>> GetAssignedMaintenanceRequestsAsync();
        Task<ServiceResult<MaintenanceRequestDto>> UpdateMaintenanceRequestAsync(int id, UpdateMaintenanceRequestDto dto);
        Task<ServiceResult<bool>> DeleteMaintenanceRequestAsync(int id);
        Task<ServiceResult<MaintenanceRequestDto>> AssignMaintenanceRequestAsync(int id, int caretakerId);
        Task<ServiceResult<MaintenanceRequestDto>> CompleteMaintenanceRequestAsync(int id, decimal actualCost, string? notes);
    }
}
