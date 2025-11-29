using AutoMapper;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Tenants;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;

namespace RentCollection.Application.Services.Implementations;

public class TenantService : ITenantService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitRepository _unitRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TenantService> _logger;

    public TenantService(
        ITenantRepository tenantRepository,
        IUnitRepository unitRepository,
        IMapper mapper,
        ILogger<TenantService> logger)
    {
        _tenantRepository = tenantRepository;
        _unitRepository = unitRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<TenantDto>>> GetAllTenantsAsync()
    {
        try
        {
            var tenants = await _tenantRepository.GetAllAsync();
            var tenantDtos = _mapper.Map<IEnumerable<TenantDto>>(tenants);

            return Result<IEnumerable<TenantDto>>.Success(tenantDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all tenants");
            return Result<IEnumerable<TenantDto>>.Failure("An error occurred while retrieving tenants");
        }
    }

    public async Task<Result<IEnumerable<TenantDto>>> GetTenantsByUnitIdAsync(int unitId)
    {
        try
        {
            var unit = await _unitRepository.GetByIdAsync(unitId);
            if (unit == null)
            {
                return Result<IEnumerable<TenantDto>>.Failure($"Unit with ID {unitId} not found");
            }

            var tenants = await _tenantRepository.GetTenantsByUnitIdAsync(unitId);
            var tenantDtos = _mapper.Map<IEnumerable<TenantDto>>(tenants);

            return Result<IEnumerable<TenantDto>>.Success(tenantDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tenants for unit {UnitId}", unitId);
            return Result<IEnumerable<TenantDto>>.Failure("An error occurred while retrieving tenants");
        }
    }

    public async Task<Result<TenantDto>> GetTenantByIdAsync(int id)
    {
        try
        {
            var tenant = await _tenantRepository.GetTenantWithDetailsAsync(id);

            if (tenant == null)
            {
                return Result<TenantDto>.Failure($"Tenant with ID {id} not found");
            }

            var tenantDto = _mapper.Map<TenantDto>(tenant);
            return Result<TenantDto>.Success(tenantDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tenant with ID {TenantId}", id);
            return Result<TenantDto>.Failure("An error occurred while retrieving the tenant");
        }
    }

    public async Task<Result<TenantDto>> CreateTenantAsync(CreateTenantDto createDto)
    {
        try
        {
            // Validate unit exists
            var unit = await _unitRepository.GetUnitWithDetailsAsync(createDto.UnitId);
            if (unit == null)
            {
                return Result<TenantDto>.Failure($"Unit with ID {createDto.UnitId} not found");
            }

            // Check if unit already has an active tenant
            var existingActiveTenants = unit.Tenants.Where(t => t.IsActive).ToList();
            if (existingActiveTenants.Any())
            {
                return Result<TenantDto>.Failure($"Unit {unit.UnitNumber} already has an active tenant. Please deactivate the current tenant first.");
            }

            // Validate lease dates
            if (createDto.LeaseEndDate.HasValue && createDto.LeaseEndDate.Value <= createDto.LeaseStartDate)
            {
                return Result<TenantDto>.Failure("Lease end date must be after lease start date");
            }

            var tenant = _mapper.Map<Tenant>(createDto);
            tenant.CreatedAt = DateTime.UtcNow;
            tenant.IsActive = true;

            var createdTenant = await _tenantRepository.AddAsync(tenant);

            // Update unit occupancy status
            unit.IsOccupied = true;
            await _unitRepository.UpdateAsync(unit);

            // Reload with details
            var tenantWithDetails = await _tenantRepository.GetTenantWithDetailsAsync(createdTenant.Id);
            var tenantDto = _mapper.Map<TenantDto>(tenantWithDetails);

            _logger.LogInformation("Tenant created successfully: {TenantName} in unit {UnitId}",
                $"{createdTenant.FirstName} {createdTenant.LastName}", createDto.UnitId);
            return Result<TenantDto>.Success(tenantDto, "Tenant created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant");
            return Result<TenantDto>.Failure("An error occurred while creating the tenant");
        }
    }

    public async Task<Result<TenantDto>> UpdateTenantAsync(int id, UpdateTenantDto updateDto)
    {
        try
        {
            var existingTenant = await _tenantRepository.GetTenantWithDetailsAsync(id);

            if (existingTenant == null)
            {
                return Result<TenantDto>.Failure($"Tenant with ID {id} not found");
            }

            // Validate lease dates
            if (updateDto.LeaseEndDate.HasValue && updateDto.LeaseEndDate.Value <= updateDto.LeaseStartDate)
            {
                return Result<TenantDto>.Failure("Lease end date must be after lease start date");
            }

            var wasActive = existingTenant.IsActive;
            _mapper.Map(updateDto, existingTenant);
            existingTenant.UpdatedAt = DateTime.UtcNow;

            await _tenantRepository.UpdateAsync(existingTenant);

            // Update unit occupancy if tenant status changed
            if (wasActive != updateDto.IsActive)
            {
                var unit = await _unitRepository.GetUnitWithDetailsAsync(existingTenant.UnitId);
                if (unit != null)
                {
                    unit.IsOccupied = unit.Tenants.Any(t => t.IsActive && t.Id != id) || updateDto.IsActive;
                    await _unitRepository.UpdateAsync(unit);
                }
            }

            var tenantDto = _mapper.Map<TenantDto>(existingTenant);

            _logger.LogInformation("Tenant updated successfully: {TenantId}", id);
            return Result<TenantDto>.Success(tenantDto, "Tenant updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant with ID {TenantId}", id);
            return Result<TenantDto>.Failure("An error occurred while updating the tenant");
        }
    }

    public async Task<Result> DeleteTenantAsync(int id)
    {
        try
        {
            var tenant = await _tenantRepository.GetTenantWithDetailsAsync(id);

            if (tenant == null)
            {
                return Result.Failure($"Tenant with ID {id} not found");
            }

            // Check if tenant has payments
            if (tenant.Payments.Any())
            {
                return Result.Failure("Cannot delete tenant with payment history. Consider deactivating the tenant instead.");
            }

            var unitId = tenant.UnitId;
            await _tenantRepository.DeleteAsync(tenant);

            // Update unit occupancy status
            var unit = await _unitRepository.GetUnitWithDetailsAsync(unitId);
            if (unit != null)
            {
                unit.IsOccupied = unit.Tenants.Any(t => t.IsActive);
                await _unitRepository.UpdateAsync(unit);
            }

            _logger.LogInformation("Tenant deleted successfully: {TenantId}", id);
            return Result.Success("Tenant deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tenant with ID {TenantId}", id);
            return Result.Failure("An error occurred while deleting the tenant");
        }
    }
}
