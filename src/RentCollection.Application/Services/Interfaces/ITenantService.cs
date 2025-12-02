using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Payments;
using RentCollection.Application.DTOs.Tenants;

namespace RentCollection.Application.Services.Interfaces;

public interface ITenantService
{
    // Existing landlord/admin methods
    Task<Result<IEnumerable<TenantDto>>> GetAllTenantsAsync();
    Task<Result<IEnumerable<TenantDto>>> GetTenantsByUnitIdAsync(int unitId);
    Task<Result<TenantDto>> GetTenantByIdAsync(int id);
    Task<Result<TenantDto>> CreateTenantAsync(CreateTenantDto createDto);
    Task<Result<TenantDto>> UpdateTenantAsync(int id, UpdateTenantDto updateDto);
    Task<Result> DeleteTenantAsync(int id);

    // Tenant application methods (public - no auth)
    Task<Result<TenantApplicationResponseDto>> SubmitApplicationAsync(TenantApplicationDto applicationDto);

    // Landlord application review methods
    Task<Result<IEnumerable<TenantApplicationResponseDto>>> GetPendingApplicationsAsync();
    Task<Result<TenantApplicationResponseDto>> GetApplicationByIdAsync(int applicationId);
    Task<Result<TenantApplicationResponseDto>> ReviewApplicationAsync(int applicationId, ReviewApplicationDto reviewDto);

    // Tenant portal methods (authenticated tenant)
    Task<Result<TenantPortalDto>> GetMyTenantInfoAsync();
    Task<Result<IEnumerable<PaymentDto>>> GetMyPaymentsAsync();
}
