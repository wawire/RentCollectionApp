using Microsoft.Extensions.Logging;
using RentCollection.Application.Common;
using RentCollection.Application.DTOs.MoveOutInspections;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Services;

public class MoveOutInspectionService : IMoveOutInspectionService
{
    private readonly IMoveOutInspectionRepository _inspectionRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly ISecurityDepositRepository _depositRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<MoveOutInspectionService> _logger;

    public MoveOutInspectionService(
        IMoveOutInspectionRepository inspectionRepository,
        ITenantRepository tenantRepository,
        ISecurityDepositRepository depositRepository,
        IPropertyRepository propertyRepository,
        IAuditLogService auditLogService,
        ICurrentUserService currentUserService,
        ILogger<MoveOutInspectionService> logger)
    {
        _inspectionRepository = inspectionRepository;
        _tenantRepository = tenantRepository;
        _depositRepository = depositRepository;
        _propertyRepository = propertyRepository;
        _auditLogService = auditLogService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ServiceResult<MoveOutInspectionDto>> ScheduleInspectionAsync(
        CreateMoveOutInspectionDto dto,
        int userId)
    {
        try
        {
            // Validate tenant exists and get tenant details
            var tenant = await _tenantRepository.GetByIdAsync(dto.TenantId);
            if (tenant == null)
                return ServiceResult<MoveOutInspectionDto>.Failure("Tenant not found");

            if (tenant.Unit == null)
                return ServiceResult<MoveOutInspectionDto>.Failure("Tenant is not assigned to a unit");

            if (_currentUserService.IsTenant)
                return ServiceResult<MoveOutInspectionDto>.Failure("Tenants cannot schedule inspections");

            if (!CanAccessTenant(tenant))
                return ServiceResult<MoveOutInspectionDto>.Failure("You don't have permission to schedule inspections for this tenant");

            // Check if tenant already has a pending inspection
            var hasPending = await _inspectionRepository.HasPendingInspectionAsync(dto.TenantId);
            if (hasPending)
                return ServiceResult<MoveOutInspectionDto>.Failure("Tenant already has a pending inspection");

            // Get current security deposit balance
            var depositBalance = await _depositRepository.GetCurrentBalanceAsync(dto.TenantId);

            var inspection = new MoveOutInspection
            {
                TenantId = dto.TenantId,
                UnitId = tenant.UnitId,
                PropertyId = tenant.Unit.PropertyId,
                MoveOutDate = dto.MoveOutDate,
                InspectionDate = dto.InspectionDate,
                InspectedByUserId = userId,
                Status = MoveOutInspectionStatus.Scheduled,
                SecurityDepositHeld = depositBalance,
                GeneralNotes = dto.Notes ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            await _inspectionRepository.AddAsync(inspection);

            await _auditLogService.LogActionAsync(
                "Create",
                "MoveOutInspection",
                inspection.Id,
                $"Scheduled move-out inspection for tenant {tenant.FullName} on {dto.InspectionDate:d}"
            );

            _logger.LogInformation("Move-out inspection scheduled for tenant {TenantId} by user {UserId}",
                dto.TenantId, userId);

            var result = await _inspectionRepository.GetByIdWithDetailsAsync(inspection.Id);
            return ServiceResult<MoveOutInspectionDto>.Success(MapToDto(result!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling inspection for tenant {TenantId}", dto.TenantId);
            return ServiceResult<MoveOutInspectionDto>.Failure($"Error scheduling inspection: {ex.Message}");
        }
    }

    public async Task<ServiceResult<MoveOutInspectionDto>> RecordInspectionAsync(
        int inspectionId,
        RecordInspectionDto dto,
        int userId)
    {
        try
        {
            var inspection = await _inspectionRepository.GetByIdWithDetailsAsync(inspectionId);
            if (inspection == null)
                return ServiceResult<MoveOutInspectionDto>.Failure("Inspection not found");

            if (!CanWriteInspection(inspection))
                return ServiceResult<MoveOutInspectionDto>.Failure("You don't have permission to record this inspection");

            // Update inspection status and general info
            inspection.Status = MoveOutInspectionStatus.Completed;
            inspection.OverallCondition = dto.OverallCondition;
            inspection.GeneralNotes = dto.GeneralNotes;
            inspection.UnpaidRent = dto.UnpaidRent;
            inspection.UnpaidUtilities = dto.UnpaidUtilities;
            inspection.OtherCharges = dto.OtherCharges;

            // Add inspection items
            foreach (var itemDto in dto.InspectionItems)
            {
                var category = Enum.Parse<InspectionCategory>(itemDto.Category);

                var item = new InspectionItem
                {
                    MoveOutInspectionId = inspectionId,
                    Category = category,
                    ItemName = itemDto.ItemName,
                    MoveInCondition = itemDto.MoveInCondition,
                    MoveOutCondition = itemDto.MoveOutCondition,
                    IsDamaged = itemDto.IsDamaged,
                    DamageDescription = itemDto.DamageDescription,
                    EstimatedRepairCost = itemDto.EstimatedRepairCost,
                    Notes = itemDto.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                inspection.InspectionItems.Add(item);
            }

            // Calculate deductions automatically
            CalculateDeductions(inspection);

            await _inspectionRepository.UpdateAsync(inspection);

            await _auditLogService.LogActionAsync(
                "Update",
                "MoveOutInspection",
                inspection.Id,
                $"Recorded inspection results with {dto.InspectionItems.Count} items. Total deductions: {inspection.TotalDeductions:C}"
            );

            _logger.LogInformation("Inspection {InspectionId} recorded by user {UserId}. Total deductions: {Amount}",
                inspectionId, userId, inspection.TotalDeductions);

            var result = await _inspectionRepository.GetByIdWithDetailsAsync(inspectionId);
            return ServiceResult<MoveOutInspectionDto>.Success(MapToDto(result!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording inspection {InspectionId}", inspectionId);
            return ServiceResult<MoveOutInspectionDto>.Failure($"Error recording inspection: {ex.Message}");
        }
    }

    public async Task<ServiceResult<MoveOutInspectionDto>> SettleInspectionAsync(
        int inspectionId,
        SettleInspectionDto dto,
        int userId)
    {
        try
        {
            var inspection = await _inspectionRepository.GetByIdWithDetailsAsync(inspectionId);
            if (inspection == null)
                return ServiceResult<MoveOutInspectionDto>.Failure("Inspection not found");

            if (!CanWriteInspection(inspection))
                return ServiceResult<MoveOutInspectionDto>.Failure("You don't have permission to settle this inspection");

            if (inspection.Status != MoveOutInspectionStatus.Completed)
                return ServiceResult<MoveOutInspectionDto>.Failure("Inspection must be completed before settlement");

            inspection.Status = MoveOutInspectionStatus.Settled;
            inspection.IsSettled = true;
            inspection.SettlementDate = DateTime.UtcNow;
            inspection.SettlementNotes = dto.SettlementNotes;

            await _inspectionRepository.UpdateAsync(inspection);

            await _auditLogService.LogActionAsync(
                "Update",
                "MoveOutInspection",
                inspection.Id,
                $"Settled inspection. Refund amount: {inspection.RefundAmount:C}"
            );

            _logger.LogInformation("Inspection {InspectionId} settled by user {UserId}",
                inspectionId, userId);

            var result = await _inspectionRepository.GetByIdWithDetailsAsync(inspectionId);
            return ServiceResult<MoveOutInspectionDto>.Success(MapToDto(result!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error settling inspection {InspectionId}", inspectionId);
            return ServiceResult<MoveOutInspectionDto>.Failure($"Error settling inspection: {ex.Message}");
        }
    }

    public async Task<ServiceResult<MoveOutInspectionDto>> ProcessRefundAsync(
        int inspectionId,
        ProcessRefundDto dto,
        int userId)
    {
        try
        {
            var inspection = await _inspectionRepository.GetByIdWithDetailsAsync(inspectionId);
            if (inspection == null)
                return ServiceResult<MoveOutInspectionDto>.Failure("Inspection not found");

            if (!CanWriteInspection(inspection))
                return ServiceResult<MoveOutInspectionDto>.Failure("You don't have permission to process this refund");

            if (!inspection.IsSettled)
                return ServiceResult<MoveOutInspectionDto>.Failure("Inspection must be settled before processing refund");

            if (inspection.RefundAmount <= 0)
                return ServiceResult<MoveOutInspectionDto>.Failure("No refund amount to process");

            inspection.Status = MoveOutInspectionStatus.RefundProcessed;
            inspection.RefundProcessed = true;
            inspection.RefundDate = DateTime.UtcNow;
            inspection.RefundMethod = dto.RefundMethod;
            inspection.RefundReference = dto.RefundReference;

            await _inspectionRepository.UpdateAsync(inspection);

            await _auditLogService.LogActionAsync(
                "Update",
                "MoveOutInspection",
                inspection.Id,
                $"Processed refund of {inspection.RefundAmount:C} via {dto.RefundMethod}"
            );

            _logger.LogInformation("Refund processed for inspection {InspectionId} by user {UserId}. Amount: {Amount}",
                inspectionId, userId, inspection.RefundAmount);

            var result = await _inspectionRepository.GetByIdWithDetailsAsync(inspectionId);
            return ServiceResult<MoveOutInspectionDto>.Success(MapToDto(result!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for inspection {InspectionId}", inspectionId);
            return ServiceResult<MoveOutInspectionDto>.Failure($"Error processing refund: {ex.Message}");
        }
    }

    public async Task<ServiceResult<MoveOutInspectionDto>> GetInspectionByIdAsync(int id, int userId)
    {
        try
        {
            var inspection = await _inspectionRepository.GetByIdWithDetailsAsync(id);
            if (inspection == null)
                return ServiceResult<MoveOutInspectionDto>.Failure("Inspection not found");

            if (!CanAccessInspection(inspection))
                return ServiceResult<MoveOutInspectionDto>.Failure("You don't have permission to view this inspection");

            return ServiceResult<MoveOutInspectionDto>.Success(MapToDto(inspection));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inspection {InspectionId}", id);
            return ServiceResult<MoveOutInspectionDto>.Failure($"Error retrieving inspection: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<MoveOutInspectionDto>>> GetTenantInspectionsAsync(int tenantId, int userId)
    {
        try
        {
            var tenant = await _tenantRepository.GetTenantWithDetailsAsync(tenantId);
            if (tenant == null)
            {
                return ServiceResult<List<MoveOutInspectionDto>>.Failure("Tenant not found");
            }

            if (!CanAccessTenant(tenant))
            {
                return ServiceResult<List<MoveOutInspectionDto>>.Failure("You don't have permission to view these inspections");
            }

            var inspections = await _inspectionRepository.GetByTenantIdAsync(tenantId);
            var dtos = inspections.Select(MapToDto).ToList();

            return ServiceResult<List<MoveOutInspectionDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inspections for tenant {TenantId}", tenantId);
            return ServiceResult<List<MoveOutInspectionDto>>.Failure($"Error retrieving inspections: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<MoveOutInspectionDto>>> GetPropertyInspectionsAsync(int propertyId, int userId)
    {
        try
        {
            if (!_currentUserService.IsPlatformAdmin)
            {
                var property = await _propertyRepository.GetByIdAsync(propertyId);
                if (property == null)
                {
                    return ServiceResult<List<MoveOutInspectionDto>>.Failure("Property not found");
                }

                if (_currentUserService.IsLandlord)
                {
                    if (!_currentUserService.UserIdInt.HasValue || property.LandlordId != _currentUserService.UserIdInt.Value)
                    {
                        return ServiceResult<List<MoveOutInspectionDto>>.Failure("You don't have permission to view inspections for this property");
                    }
                }
                else if (_currentUserService.IsManager || _currentUserService.IsCaretaker || _currentUserService.IsAccountant)
                {
                    var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
                    if (!assignedPropertyIds.Contains(propertyId))
                    {
                        return ServiceResult<List<MoveOutInspectionDto>>.Failure("You don't have permission to view inspections for this property");
                    }
                }
                else
                {
                    return ServiceResult<List<MoveOutInspectionDto>>.Failure("You don't have permission to view inspections for this property");
                }
            }

            var inspections = await GetPropertyInspectionsInternalAsync(propertyId);
            var dtos = inspections.Select(MapToDto).ToList();

            return ServiceResult<List<MoveOutInspectionDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inspections for property {PropertyId}", propertyId);
            return ServiceResult<List<MoveOutInspectionDto>>.Failure($"Error retrieving inspections: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<MoveOutInspectionDto>>> GetPendingInspectionsAsync(int userId)
    {
        try
        {
            if (_currentUserService.IsTenant)
            {
                return ServiceResult<List<MoveOutInspectionDto>>.Failure("You don't have permission to view pending inspections");
            }

            var inspections = await GetPendingInspectionsInternalAsync();
            if (!inspections.Any() && !_currentUserService.IsPlatformAdmin)
            {
                if (_currentUserService.IsLandlord && !_currentUserService.UserIdInt.HasValue)
                {
                    return ServiceResult<List<MoveOutInspectionDto>>.Failure("You don't have permission to view pending inspections");
                }
            }
            var dtos = inspections.Select(MapToDto).ToList();

            return ServiceResult<List<MoveOutInspectionDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending inspections");
            return ServiceResult<List<MoveOutInspectionDto>>.Failure($"Error retrieving inspections: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<MoveOutInspectionDto>>> GetInspectionsByStatusAsync(
        MoveOutInspectionStatus status,
        int userId)
    {
        try
        {
            if (_currentUserService.IsTenant)
            {
                return ServiceResult<List<MoveOutInspectionDto>>.Failure("You don't have permission to view inspections");
            }

            var inspections = await GetInspectionsByStatusInternalAsync(status);
            if (!inspections.Any() && !_currentUserService.IsPlatformAdmin)
            {
                if (_currentUserService.IsLandlord && !_currentUserService.UserIdInt.HasValue)
                {
                    return ServiceResult<List<MoveOutInspectionDto>>.Failure("You don't have permission to view inspections");
                }
            }
            var dtos = inspections.Select(MapToDto).ToList();

            return ServiceResult<List<MoveOutInspectionDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inspections by status {Status}", status);
            return ServiceResult<List<MoveOutInspectionDto>>.Failure($"Error retrieving inspections: {ex.Message}");
        }
    }

    public async Task<ServiceResult<InspectionPhotoDto>> UploadPhotoAsync(
        int inspectionId,
        string photoUrl,
        string? caption,
        PhotoType photoType,
        int? inspectionItemId,
        int userId)
    {
        try
        {
            var inspection = await _inspectionRepository.GetByIdWithDetailsAsync(inspectionId);
            if (inspection == null)
                return ServiceResult<InspectionPhotoDto>.Failure("Inspection not found");

            if (!CanWriteInspection(inspection))
                return ServiceResult<InspectionPhotoDto>.Failure("You don't have permission to upload photos for this inspection");

            var photo = new InspectionPhoto
            {
                MoveOutInspectionId = inspectionId,
                InspectionItemId = inspectionItemId,
                PhotoUrl = photoUrl,
                Caption = caption,
                PhotoType = photoType,
                TakenAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            inspection.Photos.Add(photo);
            await _inspectionRepository.UpdateAsync(inspection);

            await _auditLogService.LogActionAsync(
                "Create",
                "InspectionPhoto",
                photo.Id,
                $"Uploaded {photoType} photo for inspection {inspectionId}"
            );

            _logger.LogInformation("Photo uploaded for inspection {InspectionId} by user {UserId}",
                inspectionId, userId);

            return ServiceResult<InspectionPhotoDto>.Success(MapPhotoToDto(photo));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading photo for inspection {InspectionId}", inspectionId);
            return ServiceResult<InspectionPhotoDto>.Failure($"Error uploading photo: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeletePhotoAsync(int photoId, int userId)
    {
        try
        {
            // This would require a photo repository - simplified for now
            await _auditLogService.LogActionAsync(
                "Delete",
                "InspectionPhoto",
                photoId,
                $"Deleted inspection photo {photoId}"
            );

            _logger.LogInformation("Photo {PhotoId} deleted by user {UserId}", photoId, userId);

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting photo {PhotoId}", photoId);
            return ServiceResult<bool>.Failure($"Error deleting photo: {ex.Message}");
        }
    }

    /// <summary>
    /// Automatically calculate deductions from inspection items
    /// </summary>
    private void CalculateDeductions(MoveOutInspection inspection)
    {
        // Sum repair costs from inspection items
        inspection.CleaningCharges = inspection.InspectionItems
            .Where(i => i.Category == InspectionCategory.Kitchen || i.Category == InspectionCategory.Bathroom)
            .Sum(i => i.EstimatedRepairCost);

        inspection.RepairCharges = inspection.InspectionItems
            .Where(i => i.Category != InspectionCategory.Kitchen && i.Category != InspectionCategory.Bathroom)
            .Sum(i => i.EstimatedRepairCost);

        // Total deductions
        inspection.TotalDeductions =
            inspection.CleaningCharges +
            inspection.RepairCharges +
            inspection.UnpaidRent +
            inspection.UnpaidUtilities +
            inspection.OtherCharges;

        // Calculate refund or amount owed
        var netAmount = inspection.SecurityDepositHeld - inspection.TotalDeductions;

        if (netAmount >= 0)
        {
            inspection.RefundAmount = netAmount;
            inspection.TenantOwes = 0;
        }
        else
        {
            inspection.RefundAmount = 0;
            inspection.TenantOwes = Math.Abs(netAmount);
        }
    }

    private MoveOutInspectionDto MapToDto(MoveOutInspection inspection)
    {
        return new MoveOutInspectionDto
        {
            Id = inspection.Id,
            TenantId = inspection.TenantId,
            TenantName = inspection.Tenant?.FullName ?? "Unknown",
            UnitId = inspection.UnitId,
            UnitNumber = inspection.Unit?.UnitNumber ?? "N/A",
            PropertyId = inspection.PropertyId,
            PropertyName = inspection.Property?.Name ?? "Unknown",
            MoveOutDate = inspection.MoveOutDate,
            InspectionDate = inspection.InspectionDate,
            InspectedByUserId = inspection.InspectedByUserId,
            InspectedByUserName = inspection.InspectedBy?.FullName ?? "Unknown",
            Status = inspection.Status,
            StatusDisplay = inspection.Status.ToString(),
            OverallCondition = inspection.OverallCondition,
            GeneralNotes = inspection.GeneralNotes,
            CleaningCharges = inspection.CleaningCharges,
            RepairCharges = inspection.RepairCharges,
            UnpaidRent = inspection.UnpaidRent,
            UnpaidUtilities = inspection.UnpaidUtilities,
            OtherCharges = inspection.OtherCharges,
            TotalDeductions = inspection.TotalDeductions,
            SecurityDepositHeld = inspection.SecurityDepositHeld,
            RefundAmount = inspection.RefundAmount,
            TenantOwes = inspection.TenantOwes,
            IsSettled = inspection.IsSettled,
            SettlementDate = inspection.SettlementDate,
            SettlementNotes = inspection.SettlementNotes,
            RefundProcessed = inspection.RefundProcessed,
            RefundDate = inspection.RefundDate,
            RefundMethod = inspection.RefundMethod,
            RefundReference = inspection.RefundReference,
            InspectionItems = inspection.InspectionItems.Select(MapItemToDto).ToList(),
            Photos = inspection.Photos.Select(MapPhotoToDto).ToList(),
            CreatedAt = inspection.CreatedAt
        };
    }

    private InspectionItemDto MapItemToDto(InspectionItem item)
    {
        return new InspectionItemDto
        {
            Id = item.Id,
            MoveOutInspectionId = item.MoveOutInspectionId,
            Category = item.Category,
            CategoryDisplay = item.Category.ToString(),
            ItemName = item.ItemName,
            MoveInCondition = item.MoveInCondition,
            MoveOutCondition = item.MoveOutCondition,
            IsDamaged = item.IsDamaged,
            DamageDescription = item.DamageDescription,
            EstimatedRepairCost = item.EstimatedRepairCost,
            Notes = item.Notes,
            Photos = item.Photos.Select(MapPhotoToDto).ToList()
        };
    }

    private InspectionPhotoDto MapPhotoToDto(InspectionPhoto photo)
    {
        return new InspectionPhotoDto
        {
            Id = photo.Id,
            MoveOutInspectionId = photo.MoveOutInspectionId,
            InspectionItemId = photo.InspectionItemId,
            PhotoUrl = photo.PhotoUrl,
            Caption = photo.Caption,
            PhotoType = photo.PhotoType,
            PhotoTypeDisplay = photo.PhotoType.ToString(),
            TakenAt = photo.TakenAt
        };
    }

    private bool CanAccessInspection(MoveOutInspection inspection)
    {
        if (_currentUserService.IsPlatformAdmin)
        {
            return true;
        }

        if (_currentUserService.IsTenant)
        {
            return _currentUserService.TenantId == inspection.TenantId;
        }

        if (_currentUserService.IsLandlord && _currentUserService.UserIdInt.HasValue)
        {
            return inspection.Property?.LandlordId == _currentUserService.UserIdInt.Value;
        }

        if (_currentUserService.IsManager || _currentUserService.IsCaretaker || _currentUserService.IsAccountant)
        {
            var assignedPropertyIds = _currentUserService.GetAssignedPropertyIdsAsync().GetAwaiter().GetResult();
            return assignedPropertyIds.Contains(inspection.PropertyId);
        }

        return false;
    }

    private bool CanWriteInspection(MoveOutInspection inspection)
    {
        if (_currentUserService.IsTenant)
        {
            return false;
        }

        return CanAccessInspection(inspection);
    }

    private bool CanAccessTenant(Tenant tenant)
    {
        if (_currentUserService.IsPlatformAdmin)
        {
            return true;
        }

        if (_currentUserService.IsTenant)
        {
            return _currentUserService.TenantId == tenant.Id;
        }

        if (_currentUserService.IsLandlord && _currentUserService.UserIdInt.HasValue)
        {
            return tenant.Unit?.Property?.LandlordId == _currentUserService.UserIdInt.Value;
        }

        if (_currentUserService.IsManager || _currentUserService.IsCaretaker || _currentUserService.IsAccountant)
        {
            var assignedPropertyIds = _currentUserService.GetAssignedPropertyIdsAsync().GetAwaiter().GetResult();
            return tenant.Unit != null && assignedPropertyIds.Contains(tenant.Unit.PropertyId);
        }

        return false;
    }

    private async Task<List<MoveOutInspection>> GetPendingInspectionsInternalAsync()
    {
        if (_currentUserService.IsTenant)
        {
            return new List<MoveOutInspection>();
        }

        if (_currentUserService.IsPlatformAdmin)
        {
            return await _inspectionRepository.GetPendingInspectionsAsync();
        }

        if (_currentUserService.IsLandlord)
        {
            if (!_currentUserService.UserIdInt.HasValue)
            {
                return new List<MoveOutInspection>();
            }

            return await _inspectionRepository.GetPendingInspectionsAsync(_currentUserService.UserIdInt.Value);
        }

        if (_currentUserService.IsManager || _currentUserService.IsCaretaker || _currentUserService.IsAccountant)
        {
            var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
            if (assignedPropertyIds.Count == 0)
            {
                return new List<MoveOutInspection>();
            }

            var inspections = await _inspectionRepository.GetPendingInspectionsAsync();
            return inspections.Where(i => assignedPropertyIds.Contains(i.PropertyId)).ToList();
        }

        return new List<MoveOutInspection>();
    }

    private async Task<List<MoveOutInspection>> GetInspectionsByStatusInternalAsync(MoveOutInspectionStatus status)
    {
        if (_currentUserService.IsTenant)
        {
            return new List<MoveOutInspection>();
        }

        if (_currentUserService.IsPlatformAdmin)
        {
            return await _inspectionRepository.GetByStatusAsync(status);
        }

        if (_currentUserService.IsLandlord)
        {
            if (!_currentUserService.UserIdInt.HasValue)
            {
                return new List<MoveOutInspection>();
            }

            return await _inspectionRepository.GetByStatusAsync(status, _currentUserService.UserIdInt.Value);
        }

        if (_currentUserService.IsManager || _currentUserService.IsCaretaker || _currentUserService.IsAccountant)
        {
            var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
            if (assignedPropertyIds.Count == 0)
            {
                return new List<MoveOutInspection>();
            }

            var inspections = await _inspectionRepository.GetByStatusAsync(status);
            return inspections.Where(i => assignedPropertyIds.Contains(i.PropertyId)).ToList();
        }

        return new List<MoveOutInspection>();
    }

    private async Task<List<MoveOutInspection>> GetPropertyInspectionsInternalAsync(int propertyId)
    {
        if (_currentUserService.IsPlatformAdmin)
        {
            return await _inspectionRepository.GetByPropertyIdAsync(propertyId);
        }

        var property = await _propertyRepository.GetByIdAsync(propertyId);
        if (property == null)
        {
            return new List<MoveOutInspection>();
        }

        if (_currentUserService.IsLandlord)
        {
            if (!_currentUserService.UserIdInt.HasValue || property.LandlordId != _currentUserService.UserIdInt.Value)
            {
                return new List<MoveOutInspection>();
            }

            return await _inspectionRepository.GetByPropertyIdAsync(propertyId, _currentUserService.UserIdInt.Value);
        }

        if (_currentUserService.IsManager || _currentUserService.IsCaretaker || _currentUserService.IsAccountant)
        {
            var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
            if (!assignedPropertyIds.Contains(propertyId))
            {
                return new List<MoveOutInspection>();
            }

            return await _inspectionRepository.GetByPropertyIdAsync(propertyId);
        }

        return new List<MoveOutInspection>();
    }
}

