using AutoMapper;
using BCrypt.Net;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Payments;
using RentCollection.Application.DTOs.Tenants;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Services.Implementations;

public class TenantService : ITenantService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitRepository _unitRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TenantService> _logger;
    private readonly ICurrentUserService _currentUserService;

    public TenantService(
        ITenantRepository tenantRepository,
        IUnitRepository unitRepository,
        IUserRepository userRepository,
        IPaymentRepository paymentRepository,
        IMapper mapper,
        ILogger<TenantService> logger,
        ICurrentUserService currentUserService)
    {
        _tenantRepository = tenantRepository;
        _unitRepository = unitRepository;
        _userRepository = userRepository;
        _paymentRepository = paymentRepository;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IEnumerable<TenantDto>>> GetAllTenantsAsync()
    {
        try
        {
            var tenants = await _tenantRepository.GetAllAsync();

            // Filter tenants by unit's property's LandlordId (unless SystemAdmin)
            if (!_currentUserService.IsSystemAdmin)
            {
                // Tenants can only see themselves
                if (_currentUserService.IsTenant)
                {
                    if (_currentUserService.TenantId.HasValue)
                    {
                        tenants = tenants.Where(t => t.Id == _currentUserService.TenantId.Value).ToList();
                    }
                    else
                    {
                        // Tenant ID not found - return empty list
                        tenants = new List<Tenant>();
                    }
                }
                // Landlords, Caretakers, and Accountants filter by landlordId
                else
                {
                    var landlordId = _currentUserService.IsLandlord
                        ? _currentUserService.UserIdInt
                        : _currentUserService.LandlordIdInt;

                    if (landlordId.HasValue)
                    {
                        tenants = tenants.Where(t => t.Unit?.Property?.LandlordId == landlordId.Value).ToList();
                    }
                }
            }

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
            var unit = await _unitRepository.GetUnitWithDetailsAsync(unitId);
            if (unit == null)
            {
                return Result<IEnumerable<TenantDto>>.Failure($"Unit with ID {unitId} not found");
            }

            // Check access permission to the unit's property
            if (!_currentUserService.IsSystemAdmin)
            {
                // Tenants cannot access other units' tenant lists
                if (_currentUserService.IsTenant)
                {
                    return Result<IEnumerable<TenantDto>>.Failure("You do not have permission to access tenants for this unit");
                }

                // Landlords, Caretakers, and Accountants check by landlordId
                var landlordId = _currentUserService.IsLandlord
                    ? _currentUserService.UserIdInt
                    : _currentUserService.LandlordIdInt;

                if (landlordId.HasValue)
                {
                    if (unit.Property?.LandlordId != landlordId.Value)
                    {
                        return Result<IEnumerable<TenantDto>>.Failure("You do not have permission to access tenants for this unit");
                    }
                }
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

            // Check access permission via unit's property's LandlordId
            if (!_currentUserService.IsSystemAdmin)
            {
                // Tenants can only access their own record
                if (_currentUserService.IsTenant)
                {
                    if (!_currentUserService.TenantId.HasValue || tenant.Id != _currentUserService.TenantId.Value)
                    {
                        return Result<TenantDto>.Failure("You do not have permission to access this tenant");
                    }
                }
                // Landlords, Caretakers, and Accountants check by landlordId
                else
                {
                    var landlordId = _currentUserService.IsLandlord
                        ? _currentUserService.UserIdInt
                        : _currentUserService.LandlordIdInt;

                    if (landlordId.HasValue)
                    {
                        if (tenant.Unit?.Property?.LandlordId != landlordId.Value)
                        {
                            return Result<TenantDto>.Failure("You do not have permission to access this tenant");
                        }
                    }
                }
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

            // Check access permission - user must have access to the unit's property
            if (!_currentUserService.IsSystemAdmin)
            {
                var landlordId = _currentUserService.IsLandlord
                    ? _currentUserService.UserIdInt
                    : _currentUserService.LandlordIdInt;

                if (landlordId.HasValue)
                {
                    if (unit.Property?.LandlordId != landlordId.Value)
                    {
                        return Result<TenantDto>.Failure("You do not have permission to create tenants for this unit");
                    }
                }

                // Accountants cannot create tenants (read-only access)
                if (_currentUserService.IsAccountant)
                {
                    return Result<TenantDto>.Failure("Accountants do not have permission to create tenants");
                }
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

            // Check access permission via unit's property's LandlordId
            if (!_currentUserService.IsSystemAdmin)
            {
                var landlordId = _currentUserService.IsLandlord
                    ? _currentUserService.UserIdInt
                    : _currentUserService.LandlordIdInt;

                if (landlordId.HasValue)
                {
                    if (existingTenant.Unit?.Property?.LandlordId != landlordId.Value)
                    {
                        return Result<TenantDto>.Failure("You do not have permission to update this tenant");
                    }
                }

                // Accountants cannot modify tenants (read-only access)
                if (_currentUserService.IsAccountant)
                {
                    return Result<TenantDto>.Failure("Accountants do not have permission to modify tenants");
                }
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

            // Check access permission - Only SystemAdmin and Landlords can delete
            if (!_currentUserService.IsSystemAdmin && !_currentUserService.IsLandlord)
            {
                return Result.Failure("You do not have permission to delete tenants");
            }

            if (!_currentUserService.IsSystemAdmin)
            {
                var landlordId = _currentUserService.UserIdInt; // Must be landlord at this point

                if (landlordId.HasValue)
                {
                    if (tenant.Unit?.Property?.LandlordId != landlordId.Value)
                    {
                        return Result.Failure("You do not have permission to delete this tenant");
                    }
                }
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

    // ===== TENANT APPLICATION METHODS =====

    public async Task<Result<TenantApplicationResponseDto>> SubmitApplicationAsync(TenantApplicationDto applicationDto)
    {
        try
        {
            // Validate unit exists and is vacant
            var unit = await _unitRepository.GetUnitWithDetailsAsync(applicationDto.UnitId);
            if (unit == null)
            {
                return Result<TenantApplicationResponseDto>.Failure($"Unit with ID {applicationDto.UnitId} not found");
            }

            if (unit.IsOccupied)
            {
                return Result<TenantApplicationResponseDto>.Failure("This unit is currently occupied");
            }

            if (!unit.IsActive)
            {
                return Result<TenantApplicationResponseDto>.Failure("This unit is not available for rent");
            }

            // Check for duplicate email
            var existingApplications = await _tenantRepository.GetAllAsync();
            if (existingApplications.Any(t => t.Email == applicationDto.Email && t.Status == TenantStatus.Prospective))
            {
                return Result<TenantApplicationResponseDto>.Failure("An application with this email already exists");
            }

            // Create prospective tenant record
            var tenant = new Tenant
            {
                FirstName = applicationDto.FirstName,
                LastName = applicationDto.LastName,
                Email = applicationDto.Email,
                PhoneNumber = applicationDto.PhoneNumber,
                IdNumber = applicationDto.IdNumber,
                UnitId = applicationDto.UnitId,
                LeaseStartDate = applicationDto.LeaseStartDate,
                LeaseEndDate = applicationDto.LeaseEndDate,
                MonthlyRent = unit.MonthlyRent, // Use unit's rent
                SecurityDeposit = applicationDto.SecurityDeposit ?? 0,
                Status = TenantStatus.Prospective,
                ApplicationDate = DateTime.UtcNow,
                ApplicationNotes = applicationDto.ApplicationNotes,
                IsActive = false, // Not active until approved
                CreatedAt = DateTime.UtcNow
            };

            var createdTenant = await _tenantRepository.AddAsync(tenant);

            // Reload with details
            var tenantWithDetails = await _tenantRepository.GetTenantWithDetailsAsync(createdTenant.Id);

            var response = new TenantApplicationResponseDto
            {
                Id = tenantWithDetails.Id,
                FirstName = tenantWithDetails.FirstName,
                LastName = tenantWithDetails.LastName,
                Email = tenantWithDetails.Email,
                PhoneNumber = tenantWithDetails.PhoneNumber,
                IdNumber = tenantWithDetails.IdNumber,
                UnitId = tenantWithDetails.UnitId,
                UnitNumber = tenantWithDetails.Unit?.UnitNumber,
                PropertyName = tenantWithDetails.Unit?.Property?.Name,
                LeaseStartDate = tenantWithDetails.LeaseStartDate,
                LeaseEndDate = tenantWithDetails.LeaseEndDate,
                MonthlyRent = tenantWithDetails.MonthlyRent,
                SecurityDeposit = tenantWithDetails.SecurityDeposit,
                Status = tenantWithDetails.Status,
                ApplicationDate = tenantWithDetails.ApplicationDate,
                ApprovalDate = tenantWithDetails.ApprovalDate,
                ApplicationNotes = tenantWithDetails.ApplicationNotes,
                CreatedAt = tenantWithDetails.CreatedAt
            };

            _logger.LogInformation("Tenant application submitted: {Email} for unit {UnitId}",
                applicationDto.Email, applicationDto.UnitId);

            return Result<TenantApplicationResponseDto>.Success(response,
                "Application submitted successfully. The landlord will review your application.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting tenant application");
            return Result<TenantApplicationResponseDto>.Failure("An error occurred while submitting your application");
        }
    }

    public async Task<Result<IEnumerable<TenantApplicationResponseDto>>> GetPendingApplicationsAsync()
    {
        try
        {
            var allTenants = await _tenantRepository.GetAllAsync();

            // Filter by landlord's properties
            if (!_currentUserService.IsSystemAdmin)
            {
                var landlordId = _currentUserService.IsLandlord
                    ? _currentUserService.UserIdInt
                    : _currentUserService.LandlordIdInt;

                if (landlordId.HasValue)
                {
                    allTenants = allTenants.Where(t => t.Unit?.Property?.LandlordId == landlordId.Value).ToList();
                }
            }

            // Only show prospective applications
            var pendingApplications = allTenants
                .Where(t => t.Status == TenantStatus.Prospective)
                .Select(t => new TenantApplicationResponseDto
                {
                    Id = t.Id,
                    FirstName = t.FirstName,
                    LastName = t.LastName,
                    Email = t.Email,
                    PhoneNumber = t.PhoneNumber,
                    IdNumber = t.IdNumber,
                    UnitId = t.UnitId,
                    UnitNumber = t.Unit?.UnitNumber,
                    PropertyName = t.Unit?.Property?.Name,
                    LeaseStartDate = t.LeaseStartDate,
                    LeaseEndDate = t.LeaseEndDate,
                    MonthlyRent = t.MonthlyRent,
                    SecurityDeposit = t.SecurityDeposit,
                    Status = t.Status,
                    ApplicationDate = t.ApplicationDate,
                    ApprovalDate = t.ApprovalDate,
                    ApplicationNotes = t.ApplicationNotes,
                    CreatedAt = t.CreatedAt
                })
                .OrderByDescending(t => t.ApplicationDate)
                .ToList();

            return Result<IEnumerable<TenantApplicationResponseDto>>.Success(pendingApplications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending applications");
            return Result<IEnumerable<TenantApplicationResponseDto>>.Failure("An error occurred while retrieving applications");
        }
    }

    public async Task<Result<TenantApplicationResponseDto>> GetApplicationByIdAsync(int applicationId)
    {
        try
        {
            var tenant = await _tenantRepository.GetTenantWithDetailsAsync(applicationId);

            if (tenant == null)
            {
                return Result<TenantApplicationResponseDto>.Failure($"Application with ID {applicationId} not found");
            }

            // Check access permission
            if (!_currentUserService.IsSystemAdmin)
            {
                var landlordId = _currentUserService.IsLandlord
                    ? _currentUserService.UserIdInt
                    : _currentUserService.LandlordIdInt;

                if (landlordId.HasValue)
                {
                    if (tenant.Unit?.Property?.LandlordId != landlordId.Value)
                    {
                        return Result<TenantApplicationResponseDto>.Failure("You do not have permission to view this application");
                    }
                }
            }

            var response = new TenantApplicationResponseDto
            {
                Id = tenant.Id,
                FirstName = tenant.FirstName,
                LastName = tenant.LastName,
                Email = tenant.Email,
                PhoneNumber = tenant.PhoneNumber,
                IdNumber = tenant.IdNumber,
                UnitId = tenant.UnitId,
                UnitNumber = tenant.Unit?.UnitNumber,
                PropertyName = tenant.Unit?.Property?.Name,
                LeaseStartDate = tenant.LeaseStartDate,
                LeaseEndDate = tenant.LeaseEndDate,
                MonthlyRent = tenant.MonthlyRent,
                SecurityDeposit = tenant.SecurityDeposit,
                Status = tenant.Status,
                ApplicationDate = tenant.ApplicationDate,
                ApprovalDate = tenant.ApprovalDate,
                ApplicationNotes = tenant.ApplicationNotes,
                CreatedAt = tenant.CreatedAt
            };

            return Result<TenantApplicationResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving application {ApplicationId}", applicationId);
            return Result<TenantApplicationResponseDto>.Failure("An error occurred while retrieving the application");
        }
    }

    public async Task<Result<TenantApplicationResponseDto>> ReviewApplicationAsync(int applicationId, ReviewApplicationDto reviewDto)
    {
        try
        {
            var tenant = await _tenantRepository.GetTenantWithDetailsAsync(applicationId);

            if (tenant == null)
            {
                return Result<TenantApplicationResponseDto>.Failure($"Application with ID {applicationId} not found");
            }

            if (tenant.Status != TenantStatus.Prospective)
            {
                return Result<TenantApplicationResponseDto>.Failure("This application has already been reviewed");
            }

            // Check access permission
            if (!_currentUserService.IsSystemAdmin && !_currentUserService.IsLandlord)
            {
                return Result<TenantApplicationResponseDto>.Failure("You do not have permission to review applications");
            }

            if (!_currentUserService.IsSystemAdmin)
            {
                var landlordId = _currentUserService.UserIdInt;
                if (landlordId.HasValue)
                {
                    if (tenant.Unit?.Property?.LandlordId != landlordId.Value)
                    {
                        return Result<TenantApplicationResponseDto>.Failure("You do not have permission to review this application");
                    }
                }
            }

            if (reviewDto.Approved)
            {
                // Approve the application
                tenant.Status = TenantStatus.Active;
                tenant.IsActive = true;
                tenant.ApprovalDate = DateTime.UtcNow;
                tenant.Notes = reviewDto.ReviewNotes;

                // Update unit occupancy
                var unit = await _unitRepository.GetUnitWithDetailsAsync(tenant.UnitId);
                if (unit != null)
                {
                    unit.IsOccupied = true;
                    await _unitRepository.UpdateAsync(unit);
                }

                // Create user account if requested
                if (reviewDto.CreateUserAccount)
                {
                    var password = reviewDto.InitialPassword ?? GenerateRandomPassword();
                    var user = new User
                    {
                        FirstName = tenant.FirstName,
                        LastName = tenant.LastName,
                        Email = tenant.Email,
                        PhoneNumber = tenant.PhoneNumber,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                        Role = UserRole.Tenant,
                        Status = UserStatus.Active,
                        TenantId = tenant.Id,
                        CreatedAt = DateTime.UtcNow
                    };

                    var createdUser = await _userRepository.AddAsync(user);

                    _logger.LogInformation("User account created for tenant {TenantId}: {Email}", tenant.Id, tenant.Email);
                }

                await _tenantRepository.UpdateAsync(tenant);

                _logger.LogInformation("Tenant application approved: {TenantId}", applicationId);
            }
            else
            {
                // Reject the application - set to Inactive
                tenant.Status = TenantStatus.Inactive;
                tenant.IsActive = false;
                tenant.Notes = reviewDto.ReviewNotes ?? "Application rejected";
                await _tenantRepository.UpdateAsync(tenant);

                _logger.LogInformation("Tenant application rejected: {TenantId}", applicationId);
            }

            // Reload with details
            var updatedTenant = await _tenantRepository.GetTenantWithDetailsAsync(applicationId);

            var response = new TenantApplicationResponseDto
            {
                Id = updatedTenant.Id,
                FirstName = updatedTenant.FirstName,
                LastName = updatedTenant.LastName,
                Email = updatedTenant.Email,
                PhoneNumber = updatedTenant.PhoneNumber,
                IdNumber = updatedTenant.IdNumber,
                UnitId = updatedTenant.UnitId,
                UnitNumber = updatedTenant.Unit?.UnitNumber,
                PropertyName = updatedTenant.Unit?.Property?.Name,
                LeaseStartDate = updatedTenant.LeaseStartDate,
                LeaseEndDate = updatedTenant.LeaseEndDate,
                MonthlyRent = updatedTenant.MonthlyRent,
                SecurityDeposit = updatedTenant.SecurityDeposit,
                Status = updatedTenant.Status,
                ApplicationDate = updatedTenant.ApplicationDate,
                ApprovalDate = updatedTenant.ApprovalDate,
                ApplicationNotes = updatedTenant.ApplicationNotes,
                CreatedAt = updatedTenant.CreatedAt
            };

            return Result<TenantApplicationResponseDto>.Success(response,
                reviewDto.Approved ? "Application approved successfully" : "Application rejected");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing application {ApplicationId}", applicationId);
            return Result<TenantApplicationResponseDto>.Failure("An error occurred while reviewing the application");
        }
    }

    // ===== TENANT PORTAL METHODS =====

    public async Task<Result<TenantPortalDto>> GetMyTenantInfoAsync()
    {
        try
        {
            // Get current user's tenant record
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var userIdInt))
            {
                return Result<TenantPortalDto>.Failure("User not authenticated");
            }

            var user = await _userRepository.GetByIdAsync(userIdInt);
            if (user == null || user.TenantId == null)
            {
                return Result<TenantPortalDto>.Failure("Tenant record not found for current user");
            }

            var tenant = await _tenantRepository.GetTenantWithDetailsAsync(user.TenantId.Value);
            if (tenant == null)
            {
                return Result<TenantPortalDto>.Failure("Tenant record not found");
            }

            // Calculate payment summary
            var payments = await _paymentRepository.GetPaymentsByTenantIdAsync(tenant.Id);
            var totalPaid = payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount);
            var lastPayment = payments.OrderByDescending(p => p.PaymentDate).FirstOrDefault();

            // Calculate outstanding balance (simplified - months since lease start * monthly rent - total paid)
            var monthsSinceLease = (DateTime.UtcNow.Year - tenant.LeaseStartDate.Year) * 12 +
                                   (DateTime.UtcNow.Month - tenant.LeaseStartDate.Month);
            var expectedTotal = Math.Max(0, monthsSinceLease) * tenant.MonthlyRent;
            var outstanding = Math.Max(0, expectedTotal - totalPaid);

            var dto = new TenantPortalDto
            {
                Id = tenant.Id,
                FirstName = tenant.FirstName,
                LastName = tenant.LastName,
                Email = tenant.Email,
                PhoneNumber = tenant.PhoneNumber,
                IdNumber = tenant.IdNumber,
                LeaseStartDate = tenant.LeaseStartDate,
                LeaseEndDate = tenant.LeaseEndDate,
                MonthlyRent = tenant.MonthlyRent,
                SecurityDeposit = tenant.SecurityDeposit,
                Status = tenant.Status,
                UnitId = tenant.UnitId,
                UnitNumber = tenant.Unit?.UnitNumber,
                SquareFeet = tenant.Unit?.SquareFeet,
                PropertyId = tenant.Unit?.PropertyId ?? 0,
                PropertyName = tenant.Unit?.Property?.Name,
                PropertyLocation = tenant.Unit?.Property?.Location,
                TotalPaid = totalPaid,
                OutstandingBalance = outstanding,
                LastPaymentDate = lastPayment?.PaymentDate
            };

            return Result<TenantPortalDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tenant portal info");
            return Result<TenantPortalDto>.Failure("An error occurred while retrieving your information");
        }
    }

    public async Task<Result<IEnumerable<PaymentDto>>> GetMyPaymentsAsync()
    {
        try
        {
            // Get current user's tenant record
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var userIdInt))
            {
                return Result<IEnumerable<PaymentDto>>.Failure("User not authenticated");
            }

            var user = await _userRepository.GetByIdAsync(userIdInt);
            if (user == null || user.TenantId == null)
            {
                return Result<IEnumerable<PaymentDto>>.Failure("Tenant record not found for current user");
            }

            var payments = await _paymentRepository.GetPaymentsByTenantIdAsync(user.TenantId.Value);
            var paymentDtos = _mapper.Map<IEnumerable<PaymentDto>>(payments);

            return Result<IEnumerable<PaymentDto>>.Success(paymentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tenant payments");
            return Result<IEnumerable<PaymentDto>>.Failure("An error occurred while retrieving your payments");
        }
    }

    public async Task<Result<TenantDto>> UpdateMyInfoAsync(UpdateTenantSelfDto updateDto)
    {
        try
        {
            // Get current user's tenant record
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var userIdInt))
            {
                return Result<TenantDto>.Failure("User not authenticated");
            }

            var user = await _userRepository.GetByIdAsync(userIdInt);
            if (user == null || user.TenantId == null)
            {
                return Result<TenantDto>.Failure("Tenant record not found for current user");
            }

            // Get tenant
            var tenant = await _tenantRepository.GetByIdAsync(user.TenantId.Value);
            if (tenant == null)
            {
                return Result<TenantDto>.Failure("Tenant record not found");
            }

            // Update only allowed fields
            var hasChanges = false;

            if (!string.IsNullOrWhiteSpace(updateDto.PhoneNumber) && updateDto.PhoneNumber != tenant.PhoneNumber)
            {
                // Check if phone number is already used by another tenant
                var existingTenant = await _tenantRepository.GetByPhoneNumberAsync(updateDto.PhoneNumber);
                if (existingTenant != null && existingTenant.Id != tenant.Id)
                {
                    return Result<TenantDto>.Failure("Phone number is already in use");
                }

                tenant.PhoneNumber = updateDto.PhoneNumber;
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(updateDto.Email) && updateDto.Email != tenant.Email)
            {
                // Check if email is already used by another tenant
                var existingTenant = await _tenantRepository.GetByEmailAsync(updateDto.Email);
                if (existingTenant != null && existingTenant.Id != tenant.Id)
                {
                    return Result<TenantDto>.Failure("Email is already in use");
                }

                tenant.Email = updateDto.Email;
                hasChanges = true;
            }

            // Note: Notes field is not editable by tenant for security reasons
            // (it's used for landlord/admin notes about the tenant)

            if (!hasChanges)
            {
                // No changes were made
                var currentDto = _mapper.Map<TenantDto>(tenant);
                return Result<TenantDto>.Success(currentDto, "No changes were made");
            }

            // Save changes
            await _tenantRepository.UpdateAsync(tenant);

            var tenantDto = _mapper.Map<TenantDto>(tenant);
            _logger.LogInformation("Tenant {TenantId} updated their own information", tenant.Id);

            return Result<TenantDto>.Success(tenantDto, "Your information has been updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant self-information");
            return Result<TenantDto>.Failure("An error occurred while updating your information");
        }
    }

    // Helper method to generate random password
    private static string GenerateRandomPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789@#$";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
