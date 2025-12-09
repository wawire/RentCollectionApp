using RentCollection.Application.Common;
using RentCollection.Application.DTOs.LeaseRenewals;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Services.Interfaces
{
    public interface ILeaseRenewalService
    {
        Task<ServiceResult<LeaseRenewalDto>> CreateLeaseRenewalAsync(CreateLeaseRenewalDto dto);
        Task<ServiceResult<List<LeaseRenewalDto>>> GetAllLeaseRenewalsAsync();
        Task<ServiceResult<LeaseRenewalDto>> GetLeaseRenewalByIdAsync(int id);
        Task<ServiceResult<List<LeaseRenewalDto>>> GetMyLeaseRenewalsAsync();
        Task<ServiceResult<List<LeaseRenewalDto>>> GetLeaseRenewalsByPropertyIdAsync(int propertyId);
        Task<ServiceResult<List<LeaseRenewalDto>>> GetLeaseRenewalsByStatusAsync(LeaseRenewalStatus status);
        Task<ServiceResult<List<LeaseRenewalDto>>> GetExpiringSoonAsync(int daysUntilExpiry = 60);
        Task<ServiceResult<LeaseRenewalDto>> UpdateLeaseRenewalAsync(int id, UpdateLeaseRenewalDto dto);
        Task<ServiceResult<LeaseRenewalDto>> TenantRespondAsync(int id, TenantResponseDto dto);
        Task<ServiceResult<LeaseRenewalDto>> LandlordApproveAsync(int id);
        Task<ServiceResult<LeaseRenewalDto>> LandlordRejectAsync(int id, string reason);
        Task<ServiceResult<LeaseRenewalDto>> CompleteRenewalAsync(int id);
        Task<ServiceResult<bool>> DeleteLeaseRenewalAsync(int id);
    }
}
