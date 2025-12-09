using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.TenantPortal;

namespace RentCollection.Application.Services.Interfaces;

/// <summary>
/// Service for tenant portal features
/// </summary>
public interface ITenantPortalService
{
    /// <summary>
    /// Get tenant dashboard with key metrics and information
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Dashboard data</returns>
    Task<Result<TenantDashboardDto>> GetDashboardAsync(int tenantId);

    /// <summary>
    /// Get comprehensive lease information for tenant
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Lease information</returns>
    Task<Result<TenantLeaseInfoDto>> GetLeaseInfoAsync(int tenantId);

    /// <summary>
    /// Get current tenant's dashboard (from authenticated user)
    /// </summary>
    /// <returns>Dashboard data</returns>
    Task<Result<TenantDashboardDto>> GetMyDashboardAsync();

    /// <summary>
    /// Get current tenant's lease information (from authenticated user)
    /// </summary>
    /// <returns>Lease information</returns>
    Task<Result<TenantLeaseInfoDto>> GetMyLeaseInfoAsync();
}
