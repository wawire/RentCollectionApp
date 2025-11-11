using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Tenants;

namespace RentCollection.Application.Services.Interfaces;

public interface ITenantService
{
    Task<Result<IEnumerable<TenantDto>>> GetAllTenantsAsync();
    Task<Result<IEnumerable<TenantDto>>> GetTenantsByUnitIdAsync(int unitId);
    Task<Result<TenantDto>> GetTenantByIdAsync(int id);
    Task<Result<TenantDto>> CreateTenantAsync(CreateTenantDto createDto);
    Task<Result<TenantDto>> UpdateTenantAsync(int id, UpdateTenantDto updateDto);
    Task<Result> DeleteTenantAsync(int id);
}
