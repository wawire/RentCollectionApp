using RentCollection.Application.Common;
using RentCollection.Application.DTOs.MoveOutInspections;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Services.Interfaces;

public interface IMoveOutInspectionService
{
    /// <summary>
    /// Schedule a move-out inspection for a tenant
    /// </summary>
    Task<ServiceResult<MoveOutInspectionDto>> ScheduleInspectionAsync(CreateMoveOutInspectionDto dto, int userId);

    /// <summary>
    /// Record inspection results and calculate deductions
    /// </summary>
    Task<ServiceResult<MoveOutInspectionDto>> RecordInspectionAsync(int inspectionId, RecordInspectionDto dto, int userId);

    /// <summary>
    /// Settle the inspection and finalize deductions
    /// </summary>
    Task<ServiceResult<MoveOutInspectionDto>> SettleInspectionAsync(int inspectionId, SettleInspectionDto dto, int userId);

    /// <summary>
    /// Process refund to tenant via M-Pesa or other method
    /// </summary>
    Task<ServiceResult<MoveOutInspectionDto>> ProcessRefundAsync(int inspectionId, ProcessRefundDto dto, int userId);

    /// <summary>
    /// Get inspection by ID
    /// </summary>
    Task<ServiceResult<MoveOutInspectionDto>> GetInspectionByIdAsync(int id, int userId);

    /// <summary>
    /// Get all inspections for a tenant
    /// </summary>
    Task<ServiceResult<List<MoveOutInspectionDto>>> GetTenantInspectionsAsync(int tenantId, int userId);

    /// <summary>
    /// Get all inspections for a property
    /// </summary>
    Task<ServiceResult<List<MoveOutInspectionDto>>> GetPropertyInspectionsAsync(int propertyId, int userId);

    /// <summary>
    /// Get all pending inspections for landlord
    /// </summary>
    Task<ServiceResult<List<MoveOutInspectionDto>>> GetPendingInspectionsAsync(int userId);

    /// <summary>
    /// Get all inspections by status
    /// </summary>
    Task<ServiceResult<List<MoveOutInspectionDto>>> GetInspectionsByStatusAsync(MoveOutInspectionStatus status, int userId);

    /// <summary>
    /// Upload photo for inspection
    /// </summary>
    Task<ServiceResult<InspectionPhotoDto>> UploadPhotoAsync(int inspectionId, string photoUrl, string? caption, PhotoType photoType, int? inspectionItemId, int userId);

    /// <summary>
    /// Delete inspection photo
    /// </summary>
    Task<ServiceResult<bool>> DeletePhotoAsync(int photoId, int userId);
}
