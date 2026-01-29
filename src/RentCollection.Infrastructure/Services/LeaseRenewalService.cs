using Microsoft.Extensions.Logging;
using RentCollection.Application.Common;
using RentCollection.Application.DTOs.LeaseRenewals;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;

namespace RentCollection.Infrastructure.Services
{
    public class LeaseRenewalService : ILeaseRenewalService
    {
        private readonly ILeaseRenewalRepository _leaseRenewalRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly INotificationService _notificationService;
        private readonly IAuditLogService _auditLogService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<LeaseRenewalService> _logger;

        public LeaseRenewalService(
            ILeaseRenewalRepository leaseRenewalRepository,
            ITenantRepository tenantRepository,
            INotificationService notificationService,
            IAuditLogService auditLogService,
            ICurrentUserService currentUserService,
            ILogger<LeaseRenewalService> logger)
        {
            _leaseRenewalRepository = leaseRenewalRepository;
            _tenantRepository = tenantRepository;
            _notificationService = notificationService;
            _auditLogService = auditLogService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ServiceResult<LeaseRenewalDto>> CreateLeaseRenewalAsync(CreateLeaseRenewalDto dto)
        {
            try
            {
                // Only landlords, managers, and admins can create lease renewal requests
                if (!_currentUserService.IsLandlord && !_currentUserService.IsManager && !_currentUserService.IsPlatformAdmin)
                {
                    return ServiceResult<LeaseRenewalDto>.Failure("Only landlords can initiate lease renewals");
                }

                var tenant = await _tenantRepository.GetTenantWithDetailsAsync(dto.TenantId);
                if (tenant == null)
                {
                    return ServiceResult<LeaseRenewalDto>.Failure("Tenant not found");
                }

                if (!_currentUserService.IsPlatformAdmin && _currentUserService.IsManager)
                {
                    var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
                    if (tenant.Unit == null || !assignedPropertyIds.Contains(tenant.Unit.PropertyId))
                    {
                        return ServiceResult<LeaseRenewalDto>.Failure("You don't have permission to create renewals for this tenant");
                    }
                }

                // Check if tenant has an active lease
                if (!tenant.LeaseEndDate.HasValue)
                {
                    return ServiceResult<LeaseRenewalDto>.Failure("Tenant does not have an active lease");
                }

                // Check if there's already an active renewal for this tenant
                var existingRenewal = await _leaseRenewalRepository.GetActiveRenewalForTenantAsync(dto.TenantId);
                if (existingRenewal != null)
                {
                    return ServiceResult<LeaseRenewalDto>.Failure("Tenant already has an active lease renewal request");
                }

                // Calculate rent increase percentage
                decimal? rentIncreasePercentage = null;
                if (dto.ProposedRentAmount > tenant.MonthlyRent)
                {
                    rentIncreasePercentage = ((dto.ProposedRentAmount - tenant.MonthlyRent) / tenant.MonthlyRent) * 100;
                }

                var leaseRenewal = new LeaseRenewal
                {
                    TenantId = tenant.Id,
                    UnitId = tenant.UnitId,
                    PropertyId = tenant.Unit.PropertyId,
                    CurrentLeaseEndDate = tenant.LeaseEndDate.Value,
                    ProposedLeaseEndDate = dto.ProposedLeaseEndDate,
                    CurrentRentAmount = tenant.MonthlyRent,
                    ProposedRentAmount = dto.ProposedRentAmount,
                    RentIncreasePercentage = rentIncreasePercentage,
                    Status = LeaseRenewalStatus.Pending,
                    LandlordTerms = dto.LandlordTerms,
                    Notes = dto.Notes
                };

                await _leaseRenewalRepository.AddAsync(leaseRenewal);

                // Reload with details
                var createdRenewal = await _leaseRenewalRepository.GetWithDetailsAsync(leaseRenewal.Id);

                // Send notification to tenant
                // await _notificationService.SendLeaseRenewalNotificationAsync(tenant.Id, leaseRenewal.Id);

                await _auditLogService.LogActionAsync(
                    "LeaseRenewal.Create",
                    "LeaseRenewal",
                    leaseRenewal.Id,
                    $"Created lease renewal for tenant {tenant.FullName}");

                _logger.LogInformation("Lease renewal {Id} created for tenant {TenantId}",
                    leaseRenewal.Id, tenant.Id);

                return ServiceResult<LeaseRenewalDto>.Success(MapToDto(createdRenewal!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating lease renewal");
                return ServiceResult<LeaseRenewalDto>.Failure("An error occurred while creating the lease renewal");
            }
        }

        public async Task<ServiceResult<List<LeaseRenewalDto>>> GetAllLeaseRenewalsAsync()
        {
            try
            {
                var renewals = await _leaseRenewalRepository.GetAllAsync();

                // Apply RBAC filtering
                if (_currentUserService.IsTenant && _currentUserService.TenantId.HasValue)
                {
                    renewals = renewals.Where(r => r.TenantId == _currentUserService.TenantId.Value).ToList();
                }
                else if (_currentUserService.IsLandlord)
                {
                    var landlordId = _currentUserService.UserIdInt!.Value;
                    renewals = renewals.Where(r => r.Property.LandlordId == landlordId).ToList();
                }
                else if (_currentUserService.IsManager || _currentUserService.IsCaretaker || _currentUserService.IsAccountant)
                {
                    var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
                    renewals = assignedPropertyIds.Count == 0
                        ? new List<LeaseRenewal>()
                        : renewals.Where(r => assignedPropertyIds.Contains(r.PropertyId)).ToList();
                }

                var dtos = renewals.Select(MapToDto).ToList();
                return ServiceResult<List<LeaseRenewalDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lease renewals");
                return ServiceResult<List<LeaseRenewalDto>>.Failure("An error occurred while retrieving lease renewals");
            }
        }

        public async Task<ServiceResult<LeaseRenewalDto>> GetLeaseRenewalByIdAsync(int id)
        {
            try
            {
                var renewal = await _leaseRenewalRepository.GetWithDetailsAsync(id);
                if (renewal == null)
                {
                    return ServiceResult<LeaseRenewalDto>.Failure("Lease renewal not found");
                }

                // RBAC check
                if (!CanAccessRenewal(renewal))
                {
                    return ServiceResult<LeaseRenewalDto>.Failure("You don't have permission to view this lease renewal");
                }

                return ServiceResult<LeaseRenewalDto>.Success(MapToDto(renewal));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lease renewal {Id}", id);
                return ServiceResult<LeaseRenewalDto>.Failure("An error occurred while retrieving the lease renewal");
            }
        }

        public async Task<ServiceResult<List<LeaseRenewalDto>>> GetMyLeaseRenewalsAsync()
        {
            try
            {
                if (!_currentUserService.IsTenant || !_currentUserService.TenantId.HasValue)
                {
                    return ServiceResult<List<LeaseRenewalDto>>.Failure("Only tenants can view their lease renewals");
                }

                var renewals = await _leaseRenewalRepository.GetByTenantIdAsync(_currentUserService.TenantId.Value);
                var dtos = renewals.Select(MapToDto).ToList();

                return ServiceResult<List<LeaseRenewalDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tenant lease renewals");
                return ServiceResult<List<LeaseRenewalDto>>.Failure("An error occurred while retrieving your lease renewals");
            }
        }

        public async Task<ServiceResult<List<LeaseRenewalDto>>> GetLeaseRenewalsByPropertyIdAsync(int propertyId)
        {
            try
            {
                var renewals = await _leaseRenewalRepository.GetByPropertyIdAsync(propertyId);

                // Apply RBAC filtering
                if (_currentUserService.IsLandlord)
                {
                    var landlordId = _currentUserService.UserIdInt!.Value;
                    renewals = renewals.Where(r => r.Property.LandlordId == landlordId).ToList();
                }
                else if (_currentUserService.IsManager || _currentUserService.IsCaretaker || _currentUserService.IsAccountant)
                {
                    var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
                    renewals = assignedPropertyIds.Count == 0
                        ? new List<LeaseRenewal>()
                        : renewals.Where(r => assignedPropertyIds.Contains(r.PropertyId)).ToList();
                }

                var dtos = renewals.Select(MapToDto).ToList();
                return ServiceResult<List<LeaseRenewalDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lease renewals for property {PropertyId}", propertyId);
                return ServiceResult<List<LeaseRenewalDto>>.Failure("An error occurred while retrieving lease renewals");
            }
        }

        public async Task<ServiceResult<List<LeaseRenewalDto>>> GetLeaseRenewalsByStatusAsync(LeaseRenewalStatus status)
        {
            try
            {
                var renewals = await _leaseRenewalRepository.GetByStatusAsync(status);

                // Apply RBAC filtering
                if (_currentUserService.IsTenant && _currentUserService.TenantId.HasValue)
                {
                    renewals = renewals.Where(r => r.TenantId == _currentUserService.TenantId.Value).ToList();
                }
                else if (_currentUserService.IsLandlord)
                {
                    var landlordId = _currentUserService.UserIdInt!.Value;
                    renewals = renewals.Where(r => r.Property.LandlordId == landlordId).ToList();
                }
                else if (_currentUserService.IsManager || _currentUserService.IsCaretaker || _currentUserService.IsAccountant)
                {
                    var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
                    renewals = assignedPropertyIds.Count == 0
                        ? new List<LeaseRenewal>()
                        : renewals.Where(r => assignedPropertyIds.Contains(r.PropertyId)).ToList();
                }

                var dtos = renewals.Select(MapToDto).ToList();
                return ServiceResult<List<LeaseRenewalDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lease renewals by status {Status}", status);
                return ServiceResult<List<LeaseRenewalDto>>.Failure("An error occurred while retrieving lease renewals");
            }
        }

        public async Task<ServiceResult<List<LeaseRenewalDto>>> GetExpiringSoonAsync(int daysUntilExpiry = 60)
        {
            try
            {
                var renewals = await _leaseRenewalRepository.GetExpiringSoonAsync(daysUntilExpiry);

                // Apply RBAC filtering
                if (_currentUserService.IsLandlord)
                {
                    var landlordId = _currentUserService.UserIdInt!.Value;
                    renewals = renewals.Where(r => r.Property.LandlordId == landlordId).ToList();
                }
                else if (_currentUserService.IsManager || _currentUserService.IsCaretaker || _currentUserService.IsAccountant)
                {
                    var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
                    renewals = assignedPropertyIds.Count == 0
                        ? new List<LeaseRenewal>()
                        : renewals.Where(r => assignedPropertyIds.Contains(r.PropertyId)).ToList();
                }

                var dtos = renewals.Select(MapToDto).ToList();
                return ServiceResult<List<LeaseRenewalDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving expiring leases");
                return ServiceResult<List<LeaseRenewalDto>>.Failure("An error occurred while retrieving expiring leases");
            }
        }

        public async Task<ServiceResult<LeaseRenewalDto>> UpdateLeaseRenewalAsync(int id, UpdateLeaseRenewalDto dto)
        {
            try
            {
                var renewal = await _leaseRenewalRepository.GetWithDetailsAsync(id);
                if (renewal == null)
                {
                    return ServiceResult<LeaseRenewalDto>.Failure("Lease renewal not found");
                }

                // Only landlords, managers, and admins can update lease renewal details
                if (!_currentUserService.IsLandlord && !_currentUserService.IsManager && !_currentUserService.IsPlatformAdmin)
                {
                    return ServiceResult<LeaseRenewalDto>.Failure("You don't have permission to update this lease renewal");
                }

                if (_currentUserService.IsManager)
                {
                    var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
                    if (!assignedPropertyIds.Contains(renewal.PropertyId))
                    {
                        return ServiceResult<LeaseRenewalDto>.Failure("You don't have permission to update this lease renewal");
                    }
                }

                if (dto.ProposedLeaseEndDate.HasValue)
                {
                    renewal.ProposedLeaseEndDate = dto.ProposedLeaseEndDate.Value;
                }

                if (dto.ProposedRentAmount.HasValue)
                {
                    renewal.ProposedRentAmount = dto.ProposedRentAmount.Value;
                    // Recalculate rent increase percentage
                    if (dto.ProposedRentAmount.Value > renewal.CurrentRentAmount)
                    {
                        renewal.RentIncreasePercentage = ((dto.ProposedRentAmount.Value - renewal.CurrentRentAmount) / renewal.CurrentRentAmount) * 100;
                    }
                    else
                    {
                        renewal.RentIncreasePercentage = null;
                    }
                }

                if (!string.IsNullOrEmpty(dto.LandlordTerms))
                {
                    renewal.LandlordTerms = dto.LandlordTerms;
                }

                if (!string.IsNullOrEmpty(dto.Notes))
                {
                    renewal.Notes = dto.Notes;
                }

                await _leaseRenewalRepository.UpdateAsync(renewal);

                // Reload with details
                var updatedRenewal = await _leaseRenewalRepository.GetWithDetailsAsync(id);

                await _auditLogService.LogActionAsync(
                    "LeaseRenewal.Update",
                    "LeaseRenewal",
                    id,
                    "Updated lease renewal details");

                return ServiceResult<LeaseRenewalDto>.Success(MapToDto(updatedRenewal!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lease renewal {Id}", id);
                return ServiceResult<LeaseRenewalDto>.Failure("An error occurred while updating the lease renewal");
            }
        }

        public async Task<ServiceResult<LeaseRenewalDto>> TenantRespondAsync(int id, TenantResponseDto dto)
        {
            try
            {
                var renewal = await _leaseRenewalRepository.GetWithDetailsAsync(id);
                if (renewal == null)
                {
                    return ServiceResult<LeaseRenewalDto>.Failure("Lease renewal not found");
                }

                // Only the tenant can respond
                if (!_currentUserService.IsTenant || renewal.TenantId != _currentUserService.TenantId)
                {
                    return ServiceResult<LeaseRenewalDto>.Failure("You don't have permission to respond to this lease renewal");
                }

                // Can only respond if status is Pending
                if (renewal.Status != LeaseRenewalStatus.Pending)
                {
                    return ServiceResult<LeaseRenewalDto>.Failure("This lease renewal has already been responded to");
                }

                renewal.Status = dto.Accept ? LeaseRenewalStatus.TenantAccepted : LeaseRenewalStatus.TenantRejected;
                renewal.TenantResponseDate = DateTime.UtcNow;
                renewal.RejectionReason = dto.RejectionReason;

                await _leaseRenewalRepository.UpdateAsync(renewal);

                // Reload with details
                var updatedRenewal = await _leaseRenewalRepository.GetWithDetailsAsync(id);

                await _auditLogService.LogActionAsync(
                    "LeaseRenewal.TenantResponse",
                    "LeaseRenewal",
                    id,
                    $"Tenant {(dto.Accept ? "accepted" : "rejected")} lease renewal");

                _logger.LogInformation("Tenant {TenantId} {Action} lease renewal {Id}",
                    renewal.TenantId, dto.Accept ? "accepted" : "rejected", id);

                return ServiceResult<LeaseRenewalDto>.Success(MapToDto(updatedRenewal!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing tenant response for lease renewal {Id}", id);
                return ServiceResult<LeaseRenewalDto>.Failure("An error occurred while processing your response");
            }
        }

        public async Task<ServiceResult<LeaseRenewalDto>> LandlordApproveAsync(int id)
        {
            try
            {
                var renewal = await _leaseRenewalRepository.GetWithDetailsAsync(id);
                if (renewal == null)
                {
                    return ServiceResult<LeaseRenewalDto>.Failure("Lease renewal not found");
                }

                // Only landlords, managers, and admins can approve
                if (!_currentUserService.IsLandlord && !_currentUserService.IsManager && !_currentUserService.IsPlatformAdmin)
                {
                    return ServiceResult<LeaseRenewalDto>.Failure("Only landlords can approve lease renewals");
                }

                if (_currentUserService.IsManager)
                {
                    var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
                    if (!assignedPropertyIds.Contains(renewal.PropertyId))
                    {
                        return ServiceResult<LeaseRenewalDto>.Failure("Only landlords can approve lease renewals");
                    }
                }

                // Can only approve if tenant has accepted
                if (renewal.Status != LeaseRenewalStatus.TenantAccepted)
                {
                    return ServiceResult<LeaseRenewalDto>.Failure("Tenant must accept the renewal first");
                }

                renewal.Status = LeaseRenewalStatus.LandlordApproved;
                renewal.LandlordResponseDate = DateTime.UtcNow;

                await _leaseRenewalRepository.UpdateAsync(renewal);

                // Reload with details
                var updatedRenewal = await _leaseRenewalRepository.GetWithDetailsAsync(id);

                await _auditLogService.LogActionAsync(
                    "LeaseRenewal.LandlordApprove",
                    "LeaseRenewal",
                    id,
                    "Landlord approved lease renewal");

                return ServiceResult<LeaseRenewalDto>.Success(MapToDto(updatedRenewal!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving lease renewal {Id}", id);
                return ServiceResult<LeaseRenewalDto>.Failure("An error occurred while approving the lease renewal");
            }
        }

        public async Task<ServiceResult<LeaseRenewalDto>> LandlordRejectAsync(int id, string reason)
        {
            try
            {
                var renewal = await _leaseRenewalRepository.GetWithDetailsAsync(id);
                if (renewal == null)
                {
                    return ServiceResult<LeaseRenewalDto>.Failure("Lease renewal not found");
                }

                // Only landlords, managers, and admins can reject
                if (!_currentUserService.IsLandlord && !_currentUserService.IsManager && !_currentUserService.IsPlatformAdmin)
                {
                    return ServiceResult<LeaseRenewalDto>.Failure("Only landlords can reject lease renewals");
                }

                if (_currentUserService.IsManager)
                {
                    var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
                    if (!assignedPropertyIds.Contains(renewal.PropertyId))
                    {
                        return ServiceResult<LeaseRenewalDto>.Failure("Only landlords can reject lease renewals");
                    }
                }

                renewal.Status = LeaseRenewalStatus.LandlordRejected;
                renewal.LandlordResponseDate = DateTime.UtcNow;
                renewal.RejectionReason = reason;

                await _leaseRenewalRepository.UpdateAsync(renewal);

                // Reload with details
                var updatedRenewal = await _leaseRenewalRepository.GetWithDetailsAsync(id);

                await _auditLogService.LogActionAsync(
                    "LeaseRenewal.LandlordReject",
                    "LeaseRenewal",
                    id,
                    $"Landlord rejected lease renewal: {reason}");

                return ServiceResult<LeaseRenewalDto>.Success(MapToDto(updatedRenewal!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting lease renewal {Id}", id);
                return ServiceResult<LeaseRenewalDto>.Failure("An error occurred while rejecting the lease renewal");
            }
        }

        public async Task<ServiceResult<LeaseRenewalDto>> CompleteRenewalAsync(int id)
        {
            try
            {
                var renewal = await _leaseRenewalRepository.GetWithDetailsAsync(id);
                if (renewal == null)
                {
                    return ServiceResult<LeaseRenewalDto>.Failure("Lease renewal not found");
                }

                // Only landlords, managers, and admins can mark as completed
                if (!_currentUserService.IsLandlord && !_currentUserService.IsManager && !_currentUserService.IsPlatformAdmin)
                {
                    return ServiceResult<LeaseRenewalDto>.Failure("Only landlords can complete lease renewals");
                }

                if (_currentUserService.IsManager)
                {
                    var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
                    if (!assignedPropertyIds.Contains(renewal.PropertyId))
                    {
                        return ServiceResult<LeaseRenewalDto>.Failure("Only landlords can complete lease renewals");
                    }
                }

                // Can only complete if landlord has approved
                if (renewal.Status != LeaseRenewalStatus.LandlordApproved)
                {
                    return ServiceResult<LeaseRenewalDto>.Failure("Lease renewal must be approved first");
                }

                renewal.Status = LeaseRenewalStatus.Completed;
                renewal.CompletedDate = DateTime.UtcNow;

                await _leaseRenewalRepository.UpdateAsync(renewal);

                // Update tenant's lease end date and rent
                var tenant = await _tenantRepository.GetByIdAsync(renewal.TenantId);
                if (tenant != null)
                {
                    tenant.LeaseEndDate = renewal.ProposedLeaseEndDate;
                    tenant.MonthlyRent = renewal.ProposedRentAmount;
                    await _tenantRepository.UpdateAsync(tenant);
                }

                // Reload with details
                var updatedRenewal = await _leaseRenewalRepository.GetWithDetailsAsync(id);

                await _auditLogService.LogActionAsync(
                    "LeaseRenewal.Complete",
                    "LeaseRenewal",
                    id,
                    "Lease renewal completed and tenant lease updated");

                _logger.LogInformation("Lease renewal {Id} completed for tenant {TenantId}", id, renewal.TenantId);

                return ServiceResult<LeaseRenewalDto>.Success(MapToDto(updatedRenewal!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing lease renewal {Id}", id);
                return ServiceResult<LeaseRenewalDto>.Failure("An error occurred while completing the lease renewal");
            }
        }

        public async Task<ServiceResult<bool>> DeleteLeaseRenewalAsync(int id)
        {
            try
            {
                var renewal = await _leaseRenewalRepository.GetWithDetailsAsync(id);
                if (renewal == null)
                {
                    return ServiceResult<bool>.Failure("Lease renewal not found");
                }

                // Only landlords and admins can delete
                if (!_currentUserService.IsPlatformAdmin && !_currentUserService.IsLandlord)
                {
                    return ServiceResult<bool>.Failure("You don't have permission to delete this lease renewal");
                }

                // Cannot delete completed renewals
                if (renewal.Status == LeaseRenewalStatus.Completed)
                {
                    return ServiceResult<bool>.Failure("Cannot delete completed lease renewals");
                }

                await _leaseRenewalRepository.DeleteAsync(renewal);

                await _auditLogService.LogActionAsync(
                    "LeaseRenewal.Delete",
                    "LeaseRenewal",
                    id,
                    "Deleted lease renewal");

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting lease renewal {Id}", id);
                return ServiceResult<bool>.Failure("An error occurred while deleting the lease renewal");
            }
        }

        private bool CanAccessRenewal(LeaseRenewal renewal)
        {
            if (_currentUserService.IsPlatformAdmin) return true;

            if (_currentUserService.IsTenant)
            {
                return renewal.TenantId == _currentUserService.TenantId;
            }

            if (_currentUserService.IsLandlord)
            {
                return renewal.Property.LandlordId == _currentUserService.UserIdInt;
            }

            if (_currentUserService.IsManager || _currentUserService.IsCaretaker || _currentUserService.IsAccountant)
            {
                var assignedPropertyIds = _currentUserService.GetAssignedPropertyIdsAsync().GetAwaiter().GetResult();
                return assignedPropertyIds.Contains(renewal.PropertyId);
            }

            return false;
        }

        private static LeaseRenewalDto MapToDto(LeaseRenewal renewal)
        {
            return new LeaseRenewalDto
            {
                Id = renewal.Id,
                TenantId = renewal.TenantId,
                TenantName = renewal.Tenant.FullName,
                TenantEmail = renewal.Tenant.Email,
                TenantPhone = renewal.Tenant.PhoneNumber,
                UnitId = renewal.UnitId,
                UnitNumber = renewal.Unit.UnitNumber,
                PropertyId = renewal.PropertyId,
                PropertyName = renewal.Property.Name,
                CurrentLeaseEndDate = renewal.CurrentLeaseEndDate,
                ProposedLeaseEndDate = renewal.ProposedLeaseEndDate,
                CurrentRentAmount = renewal.CurrentRentAmount,
                ProposedRentAmount = renewal.ProposedRentAmount,
                RentIncreasePercentage = renewal.RentIncreasePercentage,
                Status = renewal.Status,
                StatusName = renewal.Status.ToString(),
                LandlordTerms = renewal.LandlordTerms,
                RejectionReason = renewal.RejectionReason,
                TenantResponseDate = renewal.TenantResponseDate,
                LandlordResponseDate = renewal.LandlordResponseDate,
                CompletedDate = renewal.CompletedDate,
                Notes = renewal.Notes,
                CreatedAt = renewal.CreatedAt
            };
        }
    }
}

