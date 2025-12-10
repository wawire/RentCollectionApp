using Microsoft.Extensions.Logging;
using RentCollection.Application.Common;
using RentCollection.Application.DTOs.MaintenanceRequests;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;

namespace RentCollection.Infrastructure.Services
{
    public class MaintenanceRequestService : IMaintenanceRequestService
    {
        private readonly IMaintenanceRequestRepository _maintenanceRequestRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly INotificationService _notificationService;
        private readonly IAuditLogService _auditLogService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<MaintenanceRequestService> _logger;

        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxImageSize = 5 * 1024 * 1024; // 5MB

        public MaintenanceRequestService(
            IMaintenanceRequestRepository maintenanceRequestRepository,
            ITenantRepository tenantRepository,
            IFileStorageService fileStorageService,
            INotificationService notificationService,
            IAuditLogService auditLogService,
            ICurrentUserService currentUserService,
            ILogger<MaintenanceRequestService> logger)
        {
            _maintenanceRequestRepository = maintenanceRequestRepository;
            _tenantRepository = tenantRepository;
            _fileStorageService = fileStorageService;
            _notificationService = notificationService;
            _auditLogService = auditLogService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ServiceResult<MaintenanceRequestDto>> CreateMaintenanceRequestAsync(CreateMaintenanceRequestDto dto)
        {
            try
            {
                if (!_currentUserService.IsTenant || !_currentUserService.TenantId.HasValue)
                {
                    return ServiceResult<MaintenanceRequestDto>.Failure("Only tenants can create maintenance requests");
                }

                var tenant = await _tenantRepository.GetTenantWithDetailsAsync(_currentUserService.TenantId.Value);
                if (tenant == null)
                {
                    return ServiceResult<MaintenanceRequestDto>.Failure("Tenant not found");
                }

                // Upload photos if provided
                var photoUrls = new List<string>();
                if (dto.Photos != null && dto.Photos.Any())
                {
                    if (dto.Photos.Count > 5)
                    {
                        return ServiceResult<MaintenanceRequestDto>.Failure("Maximum 5 photos allowed");
                    }

                    foreach (var photo in dto.Photos)
                    {
                        var (isValid, errorMessage) = await _fileStorageService.ValidateFileAsync(
                            photo, AllowedImageExtensions, MaxImageSize);

                        if (!isValid)
                        {
                            return ServiceResult<MaintenanceRequestDto>.Failure($"Photo validation failed: {errorMessage}");
                        }

                        var photoUrl = await _fileStorageService.UploadFileAsync(photo, "maintenance");
                        photoUrls.Add(photoUrl);
                    }
                }

                var maintenanceRequest = new MaintenanceRequest
                {
                    TenantId = tenant.Id,
                    UnitId = tenant.UnitId,
                    PropertyId = tenant.Unit.PropertyId,
                    Title = dto.Title,
                    Description = dto.Description,
                    Priority = dto.Priority,
                    Status = MaintenanceRequestStatus.Pending,
                    PhotoUrls = photoUrls.Any() ? string.Join(",", photoUrls) : null
                };

                await _maintenanceRequestRepository.AddAsync(maintenanceRequest);

                // Reload with details
                var createdRequest = await _maintenanceRequestRepository.GetWithDetailsAsync(maintenanceRequest.Id);

                // Audit log
                await _auditLogService.LogActionAsync(
                    "MaintenanceRequest.Create",
                    "MaintenanceRequest",
                    maintenanceRequest.Id,
                    $"Created: {dto.Title} (Priority: {dto.Priority})");

                _logger.LogInformation("Maintenance request {Id} created by tenant {TenantId}",
                    maintenanceRequest.Id, tenant.Id);

                return ServiceResult<MaintenanceRequestDto>.Success(MapToDto(createdRequest!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating maintenance request");
                return ServiceResult<MaintenanceRequestDto>.Failure("An error occurred while creating the maintenance request");
            }
        }

        public async Task<ServiceResult<List<MaintenanceRequestDto>>> GetAllMaintenanceRequestsAsync()
        {
            try
            {
                var requests = await _maintenanceRequestRepository.GetAllAsync();

                // Apply RBAC filtering
                if (_currentUserService.IsTenant && _currentUserService.TenantId.HasValue)
                {
                    requests = requests.Where(r => r.TenantId == _currentUserService.TenantId.Value).ToList();
                }
                else if (_currentUserService.IsLandlord)
                {
                    var landlordId = _currentUserService.UserIdInt!.Value;
                    requests = requests.Where(r => r.Property.LandlordId == landlordId).ToList();
                }
                else if (_currentUserService.IsCaretaker || _currentUserService.IsAccountant)
                {
                    var landlordId = _currentUserService.LandlordIdInt!.Value;
                    requests = requests.Where(r => r.Property.LandlordId == landlordId).ToList();
                }

                var dtos = requests.Select(MapToDto).ToList();
                return ServiceResult<List<MaintenanceRequestDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving maintenance requests");
                return ServiceResult<List<MaintenanceRequestDto>>.Failure("An error occurred while retrieving maintenance requests");
            }
        }

        public async Task<ServiceResult<MaintenanceRequestDto>> GetMaintenanceRequestByIdAsync(int id)
        {
            try
            {
                var request = await _maintenanceRequestRepository.GetWithDetailsAsync(id);
                if (request == null)
                {
                    return ServiceResult<MaintenanceRequestDto>.Failure("Maintenance request not found");
                }

                // RBAC check
                if (!CanAccessRequest(request))
                {
                    return ServiceResult<MaintenanceRequestDto>.Failure("You don't have permission to view this request");
                }

                return ServiceResult<MaintenanceRequestDto>.Success(MapToDto(request));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving maintenance request {Id}", id);
                return ServiceResult<MaintenanceRequestDto>.Failure("An error occurred while retrieving the maintenance request");
            }
        }

        public async Task<ServiceResult<List<MaintenanceRequestDto>>> GetMyMaintenanceRequestsAsync()
        {
            try
            {
                if (!_currentUserService.IsTenant || !_currentUserService.TenantId.HasValue)
                {
                    return ServiceResult<List<MaintenanceRequestDto>>.Failure("Only tenants can view their maintenance requests");
                }

                var requests = await _maintenanceRequestRepository.GetByTenantIdAsync(_currentUserService.TenantId.Value);
                var dtos = requests.Select(MapToDto).ToList();

                return ServiceResult<List<MaintenanceRequestDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tenant maintenance requests");
                return ServiceResult<List<MaintenanceRequestDto>>.Failure("An error occurred while retrieving your maintenance requests");
            }
        }

        public async Task<ServiceResult<List<MaintenanceRequestDto>>> GetMaintenanceRequestsByPropertyIdAsync(int propertyId)
        {
            try
            {
                var requests = await _maintenanceRequestRepository.GetByPropertyIdAsync(propertyId);

                // Apply RBAC filtering
                if (_currentUserService.IsLandlord)
                {
                    var landlordId = _currentUserService.UserIdInt!.Value;
                    requests = requests.Where(r => r.Property.LandlordId == landlordId).ToList();
                }

                var dtos = requests.Select(MapToDto).ToList();
                return ServiceResult<List<MaintenanceRequestDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving maintenance requests for property {PropertyId}", propertyId);
                return ServiceResult<List<MaintenanceRequestDto>>.Failure("An error occurred while retrieving maintenance requests");
            }
        }

        public async Task<ServiceResult<List<MaintenanceRequestDto>>> GetMaintenanceRequestsByStatusAsync(MaintenanceRequestStatus status)
        {
            try
            {
                var requests = await _maintenanceRequestRepository.GetByStatusAsync(status);

                // Apply RBAC filtering
                if (_currentUserService.IsTenant && _currentUserService.TenantId.HasValue)
                {
                    requests = requests.Where(r => r.TenantId == _currentUserService.TenantId.Value).ToList();
                }
                else if (_currentUserService.IsLandlord)
                {
                    var landlordId = _currentUserService.UserIdInt!.Value;
                    requests = requests.Where(r => r.Property.LandlordId == landlordId).ToList();
                }

                var dtos = requests.Select(MapToDto).ToList();
                return ServiceResult<List<MaintenanceRequestDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving maintenance requests by status {Status}", status);
                return ServiceResult<List<MaintenanceRequestDto>>.Failure("An error occurred while retrieving maintenance requests");
            }
        }

        public async Task<ServiceResult<List<MaintenanceRequestDto>>> GetAssignedMaintenanceRequestsAsync()
        {
            try
            {
                if (!_currentUserService.IsCaretaker || !_currentUserService.UserIdInt.HasValue)
                {
                    return ServiceResult<List<MaintenanceRequestDto>>.Failure("Only caretakers can view assigned requests");
                }

                var requests = await _maintenanceRequestRepository.GetByAssignedUserIdAsync(_currentUserService.UserIdInt.Value);
                var dtos = requests.Select(MapToDto).ToList();

                return ServiceResult<List<MaintenanceRequestDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assigned maintenance requests");
                return ServiceResult<List<MaintenanceRequestDto>>.Failure("An error occurred while retrieving assigned maintenance requests");
            }
        }

        public async Task<ServiceResult<MaintenanceRequestDto>> UpdateMaintenanceRequestAsync(int id, UpdateMaintenanceRequestDto dto)
        {
            try
            {
                var request = await _maintenanceRequestRepository.GetWithDetailsAsync(id);
                if (request == null)
                {
                    return ServiceResult<MaintenanceRequestDto>.Failure("Maintenance request not found");
                }

                // RBAC check
                if (!CanAccessRequest(request))
                {
                    return ServiceResult<MaintenanceRequestDto>.Failure("You don't have permission to update this request");
                }

                if (dto.Status.HasValue)
                {
                    request.Status = dto.Status.Value;
                    if (dto.Status.Value == MaintenanceRequestStatus.Completed)
                    {
                        request.CompletedAt = DateTime.UtcNow;
                    }
                }

                if (dto.Priority.HasValue) request.Priority = dto.Priority.Value;
                if (dto.AssignedToUserId.HasValue) request.AssignedToUserId = dto.AssignedToUserId.Value;
                if (dto.EstimatedCost.HasValue) request.EstimatedCost = dto.EstimatedCost.Value;
                if (dto.ActualCost.HasValue) request.ActualCost = dto.ActualCost.Value;
                if (!string.IsNullOrEmpty(dto.Notes)) request.Notes = dto.Notes;

                await _maintenanceRequestRepository.UpdateAsync(request);

                // Reload with details
                var updatedRequest = await _maintenanceRequestRepository.GetWithDetailsAsync(id);

                await _auditLogService.LogActionAsync(
                    "MaintenanceRequest.Update",
                    "MaintenanceRequest",
                    id,
                    $"Updated status to {request.Status}");

                return ServiceResult<MaintenanceRequestDto>.Success(MapToDto(updatedRequest!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating maintenance request {Id}", id);
                return ServiceResult<MaintenanceRequestDto>.Failure("An error occurred while updating the maintenance request");
            }
        }

        public async Task<ServiceResult<bool>> DeleteMaintenanceRequestAsync(int id)
        {
            try
            {
                var request = await _maintenanceRequestRepository.GetWithDetailsAsync(id);
                if (request == null)
                {
                    return ServiceResult<bool>.Failure("Maintenance request not found");
                }

                // Only tenant who created it or admin/landlord can delete
                if (!_currentUserService.IsSystemAdmin && !_currentUserService.IsLandlord)
                {
                    if (!_currentUserService.IsTenant || request.TenantId != _currentUserService.TenantId)
                    {
                        return ServiceResult<bool>.Failure("You don't have permission to delete this request");
                    }
                }

                // Delete photos if any
                if (!string.IsNullOrEmpty(request.PhotoUrls))
                {
                    var photoUrls = request.PhotoUrls.Split(',');
                    foreach (var photoUrl in photoUrls)
                    {
                        await _fileStorageService.DeleteFileAsync(photoUrl);
                    }
                }

                await _maintenanceRequestRepository.DeleteAsync(request);

                await _auditLogService.LogActionAsync(
                    "MaintenanceRequest.Delete",
                    "MaintenanceRequest",
                    id,
                    $"Deleted: {request.Title}");

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting maintenance request {Id}", id);
                return ServiceResult<bool>.Failure("An error occurred while deleting the maintenance request");
            }
        }

        public async Task<ServiceResult<MaintenanceRequestDto>> AssignMaintenanceRequestAsync(int id, int caretakerId)
        {
            try
            {
                var request = await _maintenanceRequestRepository.GetWithDetailsAsync(id);
                if (request == null)
                {
                    return ServiceResult<MaintenanceRequestDto>.Failure("Maintenance request not found");
                }

                request.AssignedToUserId = caretakerId;
                request.Status = MaintenanceRequestStatus.Assigned;

                await _maintenanceRequestRepository.UpdateAsync(request);

                var updatedRequest = await _maintenanceRequestRepository.GetWithDetailsAsync(id);

                await _auditLogService.LogActionAsync(
                    "MaintenanceRequest.Assign",
                    "MaintenanceRequest",
                    id,
                    $"Assigned to user {caretakerId}");

                return ServiceResult<MaintenanceRequestDto>.Success(MapToDto(updatedRequest!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning maintenance request {Id}", id);
                return ServiceResult<MaintenanceRequestDto>.Failure("An error occurred while assigning the maintenance request");
            }
        }

        public async Task<ServiceResult<MaintenanceRequestDto>> CompleteMaintenanceRequestAsync(int id, decimal actualCost, string? notes)
        {
            try
            {
                var request = await _maintenanceRequestRepository.GetWithDetailsAsync(id);
                if (request == null)
                {
                    return ServiceResult<MaintenanceRequestDto>.Failure("Maintenance request not found");
                }

                request.Status = MaintenanceRequestStatus.Completed;
                request.ActualCost = actualCost;
                request.Notes = notes;
                request.CompletedAt = DateTime.UtcNow;

                await _maintenanceRequestRepository.UpdateAsync(request);

                var updatedRequest = await _maintenanceRequestRepository.GetWithDetailsAsync(id);

                await _auditLogService.LogActionAsync(
                    "MaintenanceRequest.Complete",
                    "MaintenanceRequest",
                    id,
                    $"Completed with cost: {actualCost:C}");

                return ServiceResult<MaintenanceRequestDto>.Success(MapToDto(updatedRequest!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing maintenance request {Id}", id);
                return ServiceResult<MaintenanceRequestDto>.Failure("An error occurred while completing the maintenance request");
            }
        }

        private bool CanAccessRequest(MaintenanceRequest request)
        {
            if (_currentUserService.IsSystemAdmin) return true;

            if (_currentUserService.IsTenant)
            {
                return request.TenantId == _currentUserService.TenantId;
            }

            if (_currentUserService.IsLandlord)
            {
                return request.Property.LandlordId == _currentUserService.UserIdInt;
            }

            if (_currentUserService.IsCaretaker || _currentUserService.IsAccountant)
            {
                return request.Property.LandlordId == _currentUserService.LandlordIdInt;
            }

            return false;
        }

        private static MaintenanceRequestDto MapToDto(MaintenanceRequest request)
        {
            return new MaintenanceRequestDto
            {
                Id = request.Id,
                TenantId = request.TenantId,
                TenantName = request.Tenant.FullName,
                TenantPhone = request.Tenant.PhoneNumber,
                UnitId = request.UnitId,
                UnitNumber = request.Unit.UnitNumber,
                PropertyId = request.PropertyId,
                PropertyName = request.Property.Name,
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                PriorityName = request.Priority.ToString(),
                Status = request.Status,
                StatusName = request.Status.ToString(),
                PhotoUrls = !string.IsNullOrEmpty(request.PhotoUrls)
                    ? request.PhotoUrls.Split(',').ToList()
                    : new List<string>(),
                AssignedToUserId = request.AssignedToUserId,
                AssignedToName = request.AssignedToUser?.FullName,
                EstimatedCost = request.EstimatedCost,
                ActualCost = request.ActualCost,
                CreatedAt = request.CreatedAt,
                CompletedAt = request.CompletedAt,
                Notes = request.Notes
            };
        }
    }
}
