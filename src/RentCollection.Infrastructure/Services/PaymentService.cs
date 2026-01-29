using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Payments;
using RentCollection.Application.Helpers;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMapper _mapper;
    private readonly ILogger<PaymentService> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _context;
    private readonly IPaymentAllocationService _paymentAllocationService;

    public PaymentService(
        IPaymentRepository paymentRepository,
        ITenantRepository tenantRepository,
        IAuditLogService auditLogService,
        IFileStorageService fileStorageService,
        IMapper mapper,
        ILogger<PaymentService> logger,
        ICurrentUserService currentUserService,
        ApplicationDbContext context,
        IPaymentAllocationService paymentAllocationService)
    {
        _paymentRepository = paymentRepository;
        _tenantRepository = tenantRepository;
        _auditLogService = auditLogService;
        _fileStorageService = fileStorageService;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
        _context = context;
        _paymentAllocationService = paymentAllocationService;
    }

    public async Task<Result<IEnumerable<PaymentDto>>> GetAllPaymentsAsync()
    {
        try
        {
            var payments = await _paymentRepository.GetAllAsync();

            // Filter payments by tenant's unit's property's LandlordId (unless PlatformAdmin)
            if (!_currentUserService.IsPlatformAdmin)
            {
                if (!_currentUserService.OrganizationId.HasValue)
                {
                    return Result<IEnumerable<PaymentDto>>.Success(Array.Empty<PaymentDto>());
                }

                payments = payments
                    .Where(p => p.Tenant?.Unit?.Property?.OrganizationId == _currentUserService.OrganizationId.Value)
                    .ToList();

                // Tenants can only see their own payments
                if (_currentUserService.IsTenant)
                {
                    if (_currentUserService.TenantId.HasValue)
                    {
                        payments = payments.Where(p => p.TenantId == _currentUserService.TenantId.Value).ToList();
                    }
                    else
                    {
                        // Tenant ID not found - return empty list
                        payments = new List<Payment>();
                    }
                }
                // Landlords filter by landlordId, others by assigned properties
                else if (_currentUserService.IsLandlord)
                {
                    if (_currentUserService.UserIdInt.HasValue)
                    {
                        payments = payments.Where(p => p.Tenant?.Unit?.Property?.LandlordId == _currentUserService.UserIdInt.Value).ToList();
                    }
                }
                else if (_currentUserService.IsManager || _currentUserService.IsAccountant)
                {
                    var assignedPropertyIds = await GetAssignedPropertyIdsAsync();
                    if (assignedPropertyIds.Count == 0)
                    {
                        payments = new List<Payment>();
                    }
                    else
                    {
                        payments = payments.Where(p => p.Tenant?.Unit?.PropertyId != null && assignedPropertyIds.Contains(p.Tenant!.Unit!.PropertyId)).ToList();
                    }
                }
            }

            var paymentDtos = _mapper.Map<IEnumerable<PaymentDto>>(payments);

            return Result<IEnumerable<PaymentDto>>.Success(paymentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all payments");
            return Result<IEnumerable<PaymentDto>>.Failure("An error occurred while retrieving payments");
        }
    }

    public async Task<Result<IEnumerable<PaymentDto>>> GetPaymentsByTenantIdAsync(int tenantId)
    {
        try
        {
            var tenant = await _tenantRepository.GetTenantWithDetailsAsync(tenantId);
            if (tenant == null)
            {
                return Result<IEnumerable<PaymentDto>>.Failure($"Tenant with ID {tenantId} not found");
            }

            // Check access permission to the tenant's unit's property
            if (!_currentUserService.IsPlatformAdmin)
            {
                if (!IsInOrganizationScope(tenant.Unit?.Property))
                {
                    return Result<IEnumerable<PaymentDto>>.Failure("You do not have permission to access payments for this tenant");
                }

                // Tenants can only access their own payments
                if (_currentUserService.IsTenant)
                {
                    if (!_currentUserService.TenantId.HasValue || tenantId != _currentUserService.TenantId.Value)
                    {
                        return Result<IEnumerable<PaymentDto>>.Failure("You do not have permission to access payments for this tenant");
                    }
                }
                // Landlords check by landlordId, others by assigned properties
                else if (_currentUserService.IsLandlord)
                {
                    if (_currentUserService.UserIdInt.HasValue && tenant.Unit?.Property?.LandlordId != _currentUserService.UserIdInt.Value)
                    {
                        return Result<IEnumerable<PaymentDto>>.Failure("You do not have permission to access payments for this tenant");
                    }
                }
                else if (_currentUserService.IsManager || _currentUserService.IsAccountant)
                {
                    var assignedPropertyIds = await GetAssignedPropertyIdsAsync();
                    if (tenant.Unit == null || !assignedPropertyIds.Contains(tenant.Unit.PropertyId))
                    {
                        return Result<IEnumerable<PaymentDto>>.Failure("You do not have permission to access payments for this tenant");
                    }
                }
            }

            var payments = await _paymentRepository.GetPaymentsByTenantIdAsync(tenantId);
            var paymentDtos = _mapper.Map<IEnumerable<PaymentDto>>(payments);

            return Result<IEnumerable<PaymentDto>>.Success(paymentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payments for tenant {TenantId}", tenantId);
            return Result<IEnumerable<PaymentDto>>.Failure("An error occurred while retrieving payments");
        }
    }

    public async Task<Result<PaymentDto>> GetPaymentByIdAsync(int id)
    {
        try
        {
            var payment = await _paymentRepository.GetPaymentWithDetailsAsync(id);

            if (payment == null)
            {
                return Result<PaymentDto>.Failure($"Payment with ID {id} not found");
            }

            // Check access permission via tenant's unit's property's LandlordId
            if (!_currentUserService.IsPlatformAdmin)
            {
                if (!IsInOrganizationScope(payment.Tenant?.Unit?.Property))
                {
                    return Result<PaymentDto>.Failure("You do not have permission to access this payment");
                }

                // Tenants can only access their own payments
                if (_currentUserService.IsTenant)
                {
                    if (!_currentUserService.TenantId.HasValue || payment.TenantId != _currentUserService.TenantId.Value)
                    {
                        return Result<PaymentDto>.Failure("You do not have permission to access this payment");
                    }
                }
                // Landlords check by landlordId, others by assigned properties
                else if (_currentUserService.IsLandlord)
                {
                    if (_currentUserService.UserIdInt.HasValue && payment.Tenant?.Unit?.Property?.LandlordId != _currentUserService.UserIdInt.Value)
                    {
                        return Result<PaymentDto>.Failure("You do not have permission to access this payment");
                    }
                }
                else if (_currentUserService.IsManager || _currentUserService.IsAccountant)
                {
                    var assignedPropertyIds = await GetAssignedPropertyIdsAsync();
                    if (payment.Tenant?.Unit == null || !assignedPropertyIds.Contains(payment.Tenant.Unit.PropertyId))
                    {
                        return Result<PaymentDto>.Failure("You do not have permission to access this payment");
                    }
                }
            }

            var paymentDto = _mapper.Map<PaymentDto>(payment);
            return Result<PaymentDto>.Success(paymentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment with ID {PaymentId}", id);
            return Result<PaymentDto>.Failure("An error occurred while retrieving the payment");
        }
    }

    public async Task<Result<PaymentDto>> CreatePaymentAsync(CreatePaymentDto createDto)
    {
        try
        {
            // Validate tenant exists
            var tenant = await _context.Tenants
                .Include(t => t.Unit)
                    .ThenInclude(u => u.Property)
                        .ThenInclude(p => p.PaymentAccounts)
                .FirstOrDefaultAsync(t => t.Id == createDto.TenantId);
            if (tenant == null)
            {
                return Result<PaymentDto>.Failure($"Tenant with ID {createDto.TenantId} not found");
            }

            // Check access permission - user must have access to the tenant's unit's property
            if (!_currentUserService.IsPlatformAdmin)
            {
                if (!IsInOrganizationScope(tenant.Unit?.Property))
                {
                    return Result<PaymentDto>.Failure("You do not have permission to record payments for this tenant");
                }

                // Tenants cannot create payments
                if (_currentUserService.IsTenant)
                {
                    return Result<PaymentDto>.Failure("Tenants do not have permission to record payments");
                }

                // Accountants cannot record payments (read-only access)
                if (_currentUserService.IsAccountant)
                {
                    return Result<PaymentDto>.Failure("Accountants do not have permission to record payments");
                }

                if (_currentUserService.IsLandlord)
                {
                    if (_currentUserService.UserIdInt.HasValue && tenant.Unit?.Property?.LandlordId != _currentUserService.UserIdInt.Value)
                    {
                        return Result<PaymentDto>.Failure("You do not have permission to record payments for this tenant");
                    }
                }
                else if (_currentUserService.IsManager)
                {
                    var assignedPropertyIds = await GetAssignedPropertyIdsAsync();
                    if (tenant.Unit == null || !assignedPropertyIds.Contains(tenant.Unit.PropertyId))
                    {
                        return Result<PaymentDto>.Failure("You do not have permission to record payments for this tenant");
                    }
                }
                else
                {
                    return Result<PaymentDto>.Failure("You do not have permission to record payments for this tenant");
                }
            }

            if (!tenant.IsActive)
            {
                return Result<PaymentDto>.Failure("Cannot create payment for an inactive tenant");
            }

            // Validate payment amount
            if (createDto.Amount <= 0)
            {
                return Result<PaymentDto>.Failure("Payment amount must be greater than zero");
            }

            // Validate period dates
            if (createDto.PeriodEnd <= createDto.PeriodStart)
            {
                return Result<PaymentDto>.Failure("Period end date must be after period start date");
            }

            if (tenant.Unit == null)
            {
                return Result<PaymentDto>.Failure("Tenant does not have an assigned unit");
            }

            if (createDto.UnitId.HasValue && createDto.UnitId.Value != tenant.UnitId)
            {
                return Result<PaymentDto>.Failure("Unit ID does not match tenant's unit");
            }

            var paymentAccount = tenant.Unit.Property.PaymentAccounts
                .FirstOrDefault(pa => pa.Id == createDto.LandlordAccountId) ??
                tenant.Unit.Property.PaymentAccounts.FirstOrDefault(pa => pa.IsDefault && pa.IsActive) ??
                tenant.Unit.Property.PaymentAccounts.FirstOrDefault(pa => pa.IsActive);

            if (paymentAccount == null)
            {
                return Result<PaymentDto>.Failure("No active payment account configured for this property");
            }

            var dueDate = createDto.DueDate ??
                          PaymentDueDateHelper.CalculateDueDateForMonth(
                              createDto.PeriodStart.Year,
                              createDto.PeriodStart.Month,
                              tenant.RentDueDay);

            IDbContextTransaction? transaction = null;
            if (_context.Database.IsRelational())
            {
                transaction = await _context.Database.BeginTransactionAsync();
            }

            var payment = new Payment
            {
                TenantId = tenant.Id,
                UnitId = tenant.UnitId,
                LandlordAccountId = paymentAccount.Id,
                Amount = createDto.Amount,
                PaymentDate = createDto.PaymentDate,
                DueDate = dueDate,
                PaymentMethod = createDto.PaymentMethod,
                Status = PaymentStatus.Completed,
                UnallocatedAmount = createDto.Amount,
                TransactionReference = createDto.TransactionReference,
                Notes = createDto.Notes,
                PeriodStart = createDto.PeriodStart,
                PeriodEnd = createDto.PeriodEnd
            };
            payment.CreatedAt = DateTime.UtcNow;

            var createdPayment = await _paymentRepository.AddAsync(payment);

            var allocationResult = await _paymentAllocationService.AllocatePaymentToOutstandingInvoicesAsync(createdPayment.Id);
            if (!allocationResult.IsSuccess)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }

                return Result<PaymentDto>.Failure(allocationResult.ErrorMessage);
            }

            if (transaction != null)
            {
                await transaction.CommitAsync();
            }

            // Reload with details
            var paymentWithDetails = await _paymentRepository.GetPaymentWithDetailsAsync(createdPayment.Id);
            var paymentDto = _mapper.Map<PaymentDto>(paymentWithDetails);

            _logger.LogInformation("Payment recorded successfully: {Amount} for tenant {TenantId}",
                createdPayment.Amount, createDto.TenantId);
            return Result<PaymentDto>.Success(paymentDto, "Payment recorded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment");
            return Result<PaymentDto>.Failure("An error occurred while recording the payment");
        }
    }

    public async Task<Result> DeletePaymentAsync(int id)
    {
        try
        {
            var payment = await _paymentRepository.GetPaymentWithDetailsAsync(id);

            if (payment == null)
            {
                return Result.Failure($"Payment with ID {id} not found");
            }

            // Check access permission - Only PlatformAdmin and Landlords can delete
            if (!_currentUserService.IsPlatformAdmin && !_currentUserService.IsLandlord)
            {
                return Result.Failure("You do not have permission to delete payments");
            }

            if (!_currentUserService.IsPlatformAdmin)
            {
                if (!IsInOrganizationScope(payment.Tenant?.Unit?.Property))
                {
                    return Result.Failure("You do not have permission to delete this payment");
                }

                var landlordId = _currentUserService.UserIdInt; // Must be landlord at this point

                if (landlordId.HasValue)
                {
                    if (payment.Tenant?.Unit?.Property?.LandlordId != landlordId.Value)
                    {
                        return Result.Failure("You do not have permission to delete this payment");
                    }
                }
            }

            // Optionally, you can add business rules here
            // e.g., prevent deletion of payments older than X days

            await _paymentRepository.DeleteAsync(payment);

            _logger.LogInformation("Payment deleted successfully: {PaymentId}", id);
            return Result.Success("Payment deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment with ID {PaymentId}", id);
            return Result.Failure("An error occurred while deleting the payment");
        }
    }

    public async Task<Result<PaginatedList<PaymentDto>>> GetPaymentsPaginatedAsync(int pageNumber, int pageSize)
    {
        try
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Max page size

            var allPayments = await _paymentRepository.GetAllAsync();

            // Filter payments by tenant's unit's property's LandlordId (unless PlatformAdmin)
            if (!_currentUserService.IsPlatformAdmin)
            {
                if (!_currentUserService.OrganizationId.HasValue)
                {
                    return Result<PaginatedList<PaymentDto>>.Success(new PaginatedList<PaymentDto>(new List<PaymentDto>(), 0, pageNumber, pageSize));
                }

                allPayments = allPayments
                    .Where(p => p.Tenant?.Unit?.Property?.OrganizationId == _currentUserService.OrganizationId.Value)
                    .ToList();

                if (_currentUserService.IsLandlord && _currentUserService.UserIdInt.HasValue)
                {
                    allPayments = allPayments.Where(p => p.Tenant?.Unit?.Property?.LandlordId == _currentUserService.UserIdInt.Value).ToList();
                }
            else if (_currentUserService.IsManager || _currentUserService.IsAccountant)
                {
                    var assignedPropertyIds = await GetAssignedPropertyIdsAsync();
                    if (assignedPropertyIds.Count == 0)
                    {
                        allPayments = new List<Payment>();
                    }
                    else
                    {
                        allPayments = allPayments.Where(p => p.Tenant?.Unit?.PropertyId != null && assignedPropertyIds.Contains(p.Tenant!.Unit!.PropertyId)).ToList();
                    }
                }
            }

            var totalCount = allPayments.Count();

            var paginatedPayments = allPayments
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var paymentDtos = _mapper.Map<List<PaymentDto>>(paginatedPayments);
            var paginatedList = new PaginatedList<PaymentDto>(paymentDtos, totalCount, pageNumber, pageSize);

            return Result<PaginatedList<PaymentDto>>.Success(paginatedList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paginated payments");
            return Result<PaginatedList<PaymentDto>>.Failure("An error occurred while retrieving payments");
        }
    }

    public async Task<Result<PaymentInstructionsDto>> GetPaymentInstructionsAsync(int tenantId)
    {
        try
        {
            // Get tenant with unit and property details
            var tenant = await _context.Tenants
                .Include(t => t.Unit)
                    .ThenInclude(u => u.Property)
                        .ThenInclude(p => p.Landlord)
                .Include(t => t.Unit)
                    .ThenInclude(u => u.Property)
                        .ThenInclude(p => p.PaymentAccounts.Where(pa => pa.IsActive))
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
            {
                return Result<PaymentInstructionsDto>.Failure($"Tenant with ID {tenantId} not found");
            }

            if (tenant.Unit == null)
            {
                return Result<PaymentInstructionsDto>.Failure("Tenant does not have an assigned unit");
            }

            if (tenant.Unit.Property == null)
            {
                return Result<PaymentInstructionsDto>.Failure("Unit does not belong to a property");
            }

            if (!_currentUserService.IsPlatformAdmin && !IsInOrganizationScope(tenant.Unit.Property))
            {
                return Result<PaymentInstructionsDto>.Failure("You do not have permission to access payment instructions for this tenant");
            }

            // Get default payment account for the property or landlord
            var paymentAccount = tenant.Unit.Property.PaymentAccounts
                .FirstOrDefault(pa => pa.PropertyId == tenant.Unit.PropertyId && pa.IsDefault)
                ?? tenant.Unit.Property.PaymentAccounts
                    .FirstOrDefault(pa => pa.IsDefault)
                ?? tenant.Unit.Property.PaymentAccounts
                    .FirstOrDefault();

            if (paymentAccount == null)
            {
                return Result<PaymentInstructionsDto>.Failure("No payment account configured for this property");
            }

            var instructions = new PaymentInstructionsDto
            {
                TenantId = tenant.Id,
                TenantName = tenant.FullName,
                UnitId = tenant.Unit.Id,
                UnitNumber = tenant.Unit.UnitNumber,
                PropertyName = tenant.Unit.Property.Name,
                MonthlyRent = tenant.Unit.MonthlyRent,
                LandlordName = tenant.Unit.Property.Landlord?.FullName ?? "Property Owner",
                LandlordPhone = tenant.Unit.Property.Landlord?.PhoneNumber,
                LandlordAccountId = paymentAccount.Id,
                AccountType = paymentAccount.AccountType,
                AccountName = paymentAccount.AccountName,
                PaybillNumber = paymentAccount.PaybillNumber,
                PaybillName = paymentAccount.PaybillName,
                AccountNumber = tenant.Unit.PaymentAccountNumber ?? tenant.Unit.UnitNumber,
                TillNumber = paymentAccount.TillNumber,
                MPesaPhoneNumber = paymentAccount.MPesaPhoneNumber,
                BankName = paymentAccount.BankName,
                BankAccountNumber = paymentAccount.BankAccountNumber,
                BankAccountName = paymentAccount.BankAccountName,
                BankBranch = paymentAccount.BankBranch,
                ReferenceCode = $"RENT-{tenant.Unit.UnitNumber}",
                PaymentInstructions = paymentAccount.PaymentInstructions
            };

            return Result<PaymentInstructionsDto>.Success(instructions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment instructions for tenant {TenantId}", tenantId);
            return Result<PaymentInstructionsDto>.Failure("An error occurred while retrieving payment instructions");
        }
    }

    public async Task<Result<PaymentDto>> RecordTenantPaymentAsync(int tenantId, TenantRecordPaymentDto dto)
    {
        try
        {
            // Get tenant with unit details
            var tenant = await _context.Tenants
                .Include(t => t.Unit)
                    .ThenInclude(u => u.Property)
                        .ThenInclude(p => p.PaymentAccounts)
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
            {
                return Result<PaymentDto>.Failure($"Tenant with ID {tenantId} not found");
            }

            if (!tenant.IsActive)
            {
                return Result<PaymentDto>.Failure("Cannot record payment for an inactive tenant");
            }

            if (tenant.Unit == null)
            {
                return Result<PaymentDto>.Failure("Tenant does not have an assigned unit");
            }

            if (!_currentUserService.IsPlatformAdmin && !IsInOrganizationScope(tenant.Unit.Property))
            {
                return Result<PaymentDto>.Failure("You do not have permission to record payments for this tenant");
            }

            // Get the payment account
            var paymentAccount = tenant.Unit.Property.PaymentAccounts
                .FirstOrDefault(pa => pa.PropertyId == tenant.Unit.PropertyId && pa.IsDefault)
                ?? tenant.Unit.Property.PaymentAccounts
                    .FirstOrDefault(pa => pa.IsDefault)
                ?? tenant.Unit.Property.PaymentAccounts
                    .FirstOrDefault();

            if (paymentAccount == null)
            {
                return Result<PaymentDto>.Failure("No payment account configured for this property");
            }

            // Validate payment amount
            if (dto.Amount <= 0)
            {
                return Result<PaymentDto>.Failure("Payment amount must be greater than zero");
            }

            // Validate period dates
            if (dto.PeriodEnd <= dto.PeriodStart)
            {
                return Result<PaymentDto>.Failure("Period end date must be after period start date");
            }

            // Create payment record
            var dueDate = PaymentDueDateHelper.CalculateDueDateForMonth(
                dto.PeriodStart.Year,
                dto.PeriodStart.Month,
                tenant.RentDueDay);

            var payment = new Payment
            {
                TenantId = tenantId,
                UnitId = tenant.Unit.Id,
                LandlordAccountId = paymentAccount.Id,
                Amount = dto.Amount,
                PaymentDate = dto.PaymentDate,
                DueDate = dueDate,
                PaymentMethod = dto.PaymentMethod,
                Status = PaymentStatus.Pending, // Pending until landlord confirms
                UnallocatedAmount = dto.Amount,
                TransactionReference = dto.TransactionReference,
                MPesaPhoneNumber = dto.MPesaPhoneNumber,
                PaybillAccountNumber = tenant.Unit.PaymentAccountNumber ?? tenant.Unit.UnitNumber,
                Notes = dto.Notes,
                PeriodStart = dto.PeriodStart,
                PeriodEnd = dto.PeriodEnd,
                PaymentProofUrl = dto.PaymentProofUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

            // Reload with details
            var createdPayment = await _context.Payments
                .Include(p => p.Tenant)
                .Include(p => p.Unit)
                .Include(p => p.LandlordAccount)
                .FirstOrDefaultAsync(p => p.Id == payment.Id);

            var paymentDto = _mapper.Map<PaymentDto>(createdPayment);

            _logger.LogInformation("Tenant payment recorded: {Amount} for tenant {TenantId}, status: Pending",
                payment.Amount, tenantId);

            return Result<PaymentDto>.Success(paymentDto, "Payment recorded successfully. Awaiting landlord confirmation.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording tenant payment for tenant {TenantId}", tenantId);
            return Result<PaymentDto>.Failure("An error occurred while recording the payment");
        }
    }

    public async Task<Result<IEnumerable<PaymentDto>>> GetPendingPaymentsAsync(int? propertyId = null)
    {
        try
        {
            var query = _context.Payments
                .Include(p => p.Tenant)
                .Include(p => p.Unit)
                .ThenInclude(u => u.Property)
                .Include(p => p.LandlordAccount)
                .Where(p => p.Status == PaymentStatus.Pending);

            if (!_currentUserService.IsPlatformAdmin)
            {
                if (!_currentUserService.OrganizationId.HasValue)
                {
                    return Result<IEnumerable<PaymentDto>>.Failure("Assigned organization scope not found");
                }

                query = query.Where(p => p.Unit.Property.OrganizationId == _currentUserService.OrganizationId.Value);
            }

            if (_currentUserService.IsPlatformAdmin)
            {
                if (propertyId.HasValue)
                {
                    query = query.Where(p => p.Unit.PropertyId == propertyId.Value);
                }
            }
            else if (_currentUserService.IsLandlord)
            {
                var landlordId = _currentUserService.UserIdInt;
                if (!landlordId.HasValue)
                {
                    return Result<IEnumerable<PaymentDto>>.Failure("Landlord ID not found");
                }

                query = query.Where(p => p.Unit.Property.LandlordId == landlordId.Value);

                if (propertyId.HasValue)
                {
                    query = query.Where(p => p.Unit.PropertyId == propertyId.Value);
                }
            }
            else if (_currentUserService.IsManager)
            {
                var assignedPropertyIds = await GetAssignedPropertyIdsAsync();
                if (assignedPropertyIds.Count == 0)
                {
                    return Result<IEnumerable<PaymentDto>>.Failure("Assigned property scope not found");
                }

                if (propertyId.HasValue && !assignedPropertyIds.Contains(propertyId.Value))
                {
                    return Result<IEnumerable<PaymentDto>>.Failure("You do not have permission to access this property");
                }

                query = query.Where(p => assignedPropertyIds.Contains(p.Unit.PropertyId));
            }
            else
            {
                return Result<IEnumerable<PaymentDto>>.Failure("You don't have permission to view pending payments");
            }

            var payments = await query
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            var paymentDtos = _mapper.Map<IEnumerable<PaymentDto>>(payments);

            return Result<IEnumerable<PaymentDto>>.Success(paymentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending payments");
            return Result<IEnumerable<PaymentDto>>.Failure("An error occurred while retrieving pending payments");
        }
    }

    public async Task<Result<PaymentDto>> ConfirmPaymentAsync(int paymentId, int confirmedByUserId)
    {
        try
        {
            var payment = await _context.Payments
                .Include(p => p.Tenant)
                .Include(p => p.Unit)
                .ThenInclude(u => u.Property)
                .Include(p => p.LandlordAccount)
                .ThenInclude(a => a.Property)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return Result<PaymentDto>.Failure($"Payment with ID {paymentId} not found");
            }

            if (payment.Status != PaymentStatus.Pending)
            {
                return Result<PaymentDto>.Failure($"Payment is already {payment.Status}. Only pending payments can be confirmed.");
            }

            if (_currentUserService.UserIdInt.HasValue && _currentUserService.UserIdInt.Value != confirmedByUserId)
            {
                return Result<PaymentDto>.Failure("ConfirmedByUserId must match the authenticated user");
            }

            if (!_currentUserService.IsPlatformAdmin)
            {
                if (!IsInOrganizationScope(payment.Unit?.Property))
                {
                    return Result<PaymentDto>.Failure("You do not have permission to confirm this payment");
                }

                if (_currentUserService.IsLandlord)
                {
                    var landlordId = _currentUserService.UserIdInt;
                    if (!landlordId.HasValue || payment.Unit.Property.LandlordId != landlordId.Value)
                    {
                        return Result<PaymentDto>.Failure("You do not have permission to confirm this payment");
                    }
                }
                else if (_currentUserService.IsManager)
                {
                    var assignedPropertyIds = await GetAssignedPropertyIdsAsync();
                    if (payment.Unit.PropertyId == 0 || !assignedPropertyIds.Contains(payment.Unit.PropertyId))
                    {
                        return Result<PaymentDto>.Failure("You do not have permission to confirm this payment");
                    }
                }
                else
                {
                    return Result<PaymentDto>.Failure("You do not have permission to confirm this payment");
                }
            }

            IDbContextTransaction? transaction = null;
            if (_context.Database.IsRelational())
            {
                transaction = await _context.Database.BeginTransactionAsync();
            }

            payment.Status = PaymentStatus.Completed;
            payment.ConfirmedAt = DateTime.UtcNow;
            payment.ConfirmedByUserId = confirmedByUserId;
            payment.UpdatedAt = DateTime.UtcNow;

            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();

            var allocationResult = await _paymentAllocationService.AllocatePaymentToOutstandingInvoicesAsync(payment.Id);
            if (!allocationResult.IsSuccess)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }
                return Result<PaymentDto>.Failure(allocationResult.ErrorMessage);
            }

            if (transaction != null)
            {
                await transaction.CommitAsync();
            }

            var paymentDto = _mapper.Map<PaymentDto>(payment);

            _logger.LogInformation("Payment {PaymentId} confirmed by user {UserId}", paymentId, confirmedByUserId);

            // Audit log: Payment confirmed
            await _auditLogService.LogActionAsync(
                "Confirm",
                "Payment",
                paymentId,
                $"Payment of KES {payment.Amount:N2} for Tenant#{payment.TenantId} confirmed");

            return Result<PaymentDto>.Success(paymentDto, "Payment confirmed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming payment {PaymentId}", paymentId);
            return Result<PaymentDto>.Failure("An error occurred while confirming the payment");
        }
    }

    public async Task<Result<PaymentDto>> RejectPaymentAsync(int paymentId, string reason)
    {
        try
        {
            var payment = await _context.Payments
                .Include(p => p.Tenant)
                .Include(p => p.Unit)
                .ThenInclude(u => u.Property)
                .Include(p => p.LandlordAccount)
                .ThenInclude(a => a.Property)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return Result<PaymentDto>.Failure($"Payment with ID {paymentId} not found");
            }

            if (payment.Status != PaymentStatus.Pending)
            {
                return Result<PaymentDto>.Failure($"Payment is already {payment.Status}. Only pending payments can be rejected.");
            }

            if (!_currentUserService.IsPlatformAdmin)
            {
                if (!IsInOrganizationScope(payment.Unit?.Property))
                {
                    return Result<PaymentDto>.Failure("You do not have permission to reject this payment");
                }

                if (_currentUserService.IsLandlord)
                {
                    var landlordId = _currentUserService.UserIdInt;
                    if (!landlordId.HasValue || payment.Unit.Property.LandlordId != landlordId.Value)
                    {
                        return Result<PaymentDto>.Failure("You do not have permission to reject this payment");
                    }
                }
                else if (_currentUserService.IsManager)
                {
                    var assignedPropertyIds = await GetAssignedPropertyIdsAsync();
                    if (payment.Unit.PropertyId == 0 || !assignedPropertyIds.Contains(payment.Unit.PropertyId))
                    {
                        return Result<PaymentDto>.Failure("You do not have permission to reject this payment");
                    }
                }
                else
                {
                    return Result<PaymentDto>.Failure("You do not have permission to reject this payment");
                }
            }

            // Update payment status
            payment.Status = PaymentStatus.Rejected;
            payment.Notes = string.IsNullOrEmpty(payment.Notes)
                ? $"Rejected: {reason}"
                : $"{payment.Notes}\n\nRejected: {reason}";
            payment.UpdatedAt = DateTime.UtcNow;

            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();

            var paymentDto = _mapper.Map<PaymentDto>(payment);

            _logger.LogInformation("Payment {PaymentId} rejected. Reason: {Reason}", paymentId, reason);

            // Audit log: Payment rejected
            await _auditLogService.LogActionAsync(
                "Reject",
                "Payment",
                paymentId,
                $"Payment of KES {payment.Amount:N2} for Tenant#{payment.TenantId} rejected. Reason: {reason}");

            return Result<PaymentDto>.Success(paymentDto, "Payment rejected");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting payment {PaymentId}", paymentId);
            return Result<PaymentDto>.Failure("An error occurred while rejecting the payment");
        }
    }

    public async Task<Result<PaymentDto>> UploadPaymentProofAsync(int paymentId, int tenantId, IFormFile file)
    {
        try
        {
            var payment = await _context.Payments
                .Include(p => p.Tenant)
                .Include(p => p.Unit)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return Result<PaymentDto>.Failure($"Payment with ID {paymentId} not found");
            }

            // Verify payment belongs to tenant
            if (payment.TenantId != tenantId)
            {
                return Result<PaymentDto>.Failure("You do not have permission to upload proof for this payment");
            }

            // Validate file (images and PDFs)
            var (isValid, errorMessage) = await _fileStorageService.ValidateFileAsync(
                file,
                allowedExtensions: new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" },
                maxSizeInBytes: 10 * 1024 * 1024 // 10MB max for payment proofs
            );

            if (!isValid)
            {
                return Result<PaymentDto>.Failure(errorMessage);
            }

            // Delete old proof if exists
            if (!string.IsNullOrEmpty(payment.PaymentProofUrl))
            {
                await _fileStorageService.DeleteFileAsync(payment.PaymentProofUrl);
            }

            // Upload new proof
            var proofUrl = await _fileStorageService.UploadFileAsync(file, "payment-proofs");

            // Update payment
            payment.PaymentProofUrl = proofUrl;
            payment.UpdatedAt = DateTime.UtcNow;

            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();

            var paymentDto = _mapper.Map<PaymentDto>(payment);

            _logger.LogInformation("Payment proof uploaded: Payment#{PaymentId}, Tenant#{TenantId}, URL: {ProofUrl}",
                paymentId, tenantId, proofUrl);

            return Result<PaymentDto>.Success(paymentDto, "Payment proof uploaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading payment proof for payment {PaymentId}", paymentId);
            return Result<PaymentDto>.Failure("An error occurred while uploading the payment proof");
        }
    }

    public async Task<Result<IEnumerable<PaymentDto>>> GetOverduePaymentsAsync(int? propertyId = null)
    {
        try
        {
            IEnumerable<Payment> overduePayments;

            // RBAC: PlatformAdmin can see all overdue payments
            if (_currentUserService.IsPlatformAdmin)
            {
                if (propertyId.HasValue)
                {
                    overduePayments = await _paymentRepository.GetOverduePaymentsByPropertyIdAsync(propertyId.Value);
                }
                else
                {
                    overduePayments = await _paymentRepository.GetOverduePaymentsAsync();
                }
            }
            // Landlord can only see overdue payments for their properties
            else if (_currentUserService.IsLandlord)
            {
                var landlordId = _currentUserService.UserIdInt;
                if (!landlordId.HasValue)
                {
                    return Result<IEnumerable<PaymentDto>>.Failure("Landlord ID not found");
                }

                if (propertyId.HasValue)
                {
                    // Verify landlord owns this property
                    var property = await _context.Properties
                        .FirstOrDefaultAsync(p => p.Id == propertyId.Value && p.LandlordId == landlordId.Value);

                    if (property == null)
                    {
                        return Result<IEnumerable<PaymentDto>>.Failure("Property not found or you don't have permission to view it");
                    }

                    overduePayments = await _paymentRepository.GetOverduePaymentsByPropertyIdAsync(propertyId.Value);
                }
                else
                {
                    // Get all overdue payments for landlord's properties
                    overduePayments = await _paymentRepository.GetOverduePaymentsByLandlordIdAsync(landlordId.Value);
                }
            }
            // Managers can see overdue payments for assigned properties
            else if (_currentUserService.IsManager)
            {
                var assignedPropertyIds = await GetAssignedPropertyIdsAsync();
                if (assignedPropertyIds.Count == 0)
                {
                    return Result<IEnumerable<PaymentDto>>.Failure("Assigned property scope not found");
                }

                if (propertyId.HasValue)
                {
                    if (!assignedPropertyIds.Contains(propertyId.Value))
                    {
                        return Result<IEnumerable<PaymentDto>>.Failure("Property not found or you don't have permission to view it");
                    }

                    overduePayments = await _paymentRepository.GetOverduePaymentsByPropertyIdAsync(propertyId.Value);
                }
                else
                {
                    overduePayments = await _paymentRepository.GetOverduePaymentsByPropertyIdsAsync(assignedPropertyIds);
                }
            }
            // Accountant can see overdue payments for assigned properties
            else if (_currentUserService.IsAccountant)
            {
                var assignedPropertyIds = await GetAssignedPropertyIdsAsync();
                if (assignedPropertyIds.Count == 0)
                {
                    return Result<IEnumerable<PaymentDto>>.Failure("Assigned property scope not found for accountant");
                }

                if (propertyId.HasValue)
                {
                    if (!assignedPropertyIds.Contains(propertyId.Value))
                    {
                        return Result<IEnumerable<PaymentDto>>.Failure("Property not found or you don't have permission to view it");
                    }

                    overduePayments = await _paymentRepository.GetOverduePaymentsByPropertyIdAsync(propertyId.Value);
                }
                else
                {
                    overduePayments = await _paymentRepository.GetOverduePaymentsByPropertyIdsAsync(assignedPropertyIds);
                }
            }
            else
            {
                return Result<IEnumerable<PaymentDto>>.Failure("You don't have permission to view overdue payments");
            }

            var paymentDtos = _mapper.Map<IEnumerable<PaymentDto>>(overduePayments);

            _logger.LogInformation("Retrieved {Count} overdue payments", paymentDtos.Count());

            return Result<IEnumerable<PaymentDto>>.Success(paymentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue payments");
            return Result<IEnumerable<PaymentDto>>.Failure("An error occurred while retrieving overdue payments");
        }
    }

    public async Task<Result<PaymentDto>> ApplyLateFeeAsync(int paymentId)
    {
        try
        {
            var payment = await _context.Payments
                .Include(p => p.Tenant)
                    .ThenInclude(t => t.Unit)
                        .ThenInclude(u => u.Property)
                .Include(p => p.Unit)
                    .ThenInclude(u => u.Property)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return Result<PaymentDto>.Failure($"Payment with ID {paymentId} not found");
            }

            if (!_currentUserService.IsPlatformAdmin)
            {
                if (!IsInOrganizationScope(payment.Unit?.Property))
                {
                    return Result<PaymentDto>.Failure("You do not have permission to access this payment");
                }

                if (_currentUserService.IsTenant)
                {
                    if (!_currentUserService.TenantId.HasValue || payment.TenantId != _currentUserService.TenantId.Value)
                    {
                        return Result<PaymentDto>.Failure("You do not have permission to access this payment");
                    }
                }
                else if (_currentUserService.IsLandlord)
                {
                    if (!_currentUserService.UserIdInt.HasValue || payment.Unit?.Property?.LandlordId != _currentUserService.UserIdInt.Value)
                    {
                        return Result<PaymentDto>.Failure("You do not have permission to access this payment");
                    }
                }
                else if (_currentUserService.IsManager || _currentUserService.IsAccountant)
                {
                    var assignedPropertyIds = await GetAssignedPropertyIdsAsync();
                    if (payment.Unit == null || !assignedPropertyIds.Contains(payment.Unit.PropertyId))
                    {
                        return Result<PaymentDto>.Failure("You do not have permission to access this payment");
                    }
                }
                else
                {
                    return Result<PaymentDto>.Failure("You do not have permission to access this payment");
                }
            }

            // Check if payment is pending
            if (payment.Status != PaymentStatus.Pending)
            {
                return Result<PaymentDto>.Failure("Late fees can only be applied to pending payments");
            }

            // Check if already has late fee
            if (payment.LateFeeAmount > 0)
            {
                return Result<PaymentDto>.Failure("Late fee has already been applied to this payment");
            }

            // Calculate late fee
            var lateFee = LateFeeCalculator.CalculateCurrentLateFee(payment.Tenant, payment.DueDate);

            if (lateFee == 0)
            {
                return Result<PaymentDto>.Failure("No late fee applicable - payment is within grace period or not overdue");
            }

            // Apply late fee
            payment.LateFeeAmount = lateFee;
            payment.UpdatedAt = DateTime.UtcNow;

            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();

            var paymentDto = _mapper.Map<PaymentDto>(payment);

            _logger.LogInformation("Late fee of KES {LateFee} applied to payment {PaymentId}", lateFee, paymentId);

            return Result<PaymentDto>.Success(paymentDto, $"Late fee of KES {lateFee:N2} applied successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying late fee to payment {PaymentId}", paymentId);
            return Result<PaymentDto>.Failure("An error occurred while applying the late fee");
        }
    }

    public async Task<Result<LateFeeCalculationDto>> CalculateLateFeeAsync(int paymentId)
    {
        try
        {
            var payment = await _context.Payments
                .Include(p => p.Tenant)
                    .ThenInclude(t => t.Unit)
                        .ThenInclude(u => u.Property)
                .Include(p => p.Unit)
                    .ThenInclude(u => u.Property)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return Result<LateFeeCalculationDto>.Failure($"Payment with ID {paymentId} not found");
            }

            if (!_currentUserService.IsPlatformAdmin)
            {
                if (!IsInOrganizationScope(payment.Unit?.Property))
                {
                    return Result<LateFeeCalculationDto>.Failure("You do not have permission to access this payment");
                }

                if (_currentUserService.IsTenant)
                {
                    if (!_currentUserService.TenantId.HasValue || payment.TenantId != _currentUserService.TenantId.Value)
                    {
                        return Result<LateFeeCalculationDto>.Failure("You do not have permission to access this payment");
                    }
                }
                else if (_currentUserService.IsLandlord)
                {
                    if (!_currentUserService.UserIdInt.HasValue || payment.Unit?.Property?.LandlordId != _currentUserService.UserIdInt.Value)
                    {
                        return Result<LateFeeCalculationDto>.Failure("You do not have permission to access this payment");
                    }
                }
                else if (_currentUserService.IsManager || _currentUserService.IsAccountant)
                {
                    var assignedPropertyIds = await GetAssignedPropertyIdsAsync();
                    if (payment.Unit == null || !assignedPropertyIds.Contains(payment.Unit.PropertyId))
                    {
                        return Result<LateFeeCalculationDto>.Failure("You do not have permission to access this payment");
                    }
                }
                else
                {
                    return Result<LateFeeCalculationDto>.Failure("You do not have permission to access this payment");
                }
            }

            var currentDate = DateTime.UtcNow;
            var daysOverdue = (currentDate.Date - payment.DueDate.Date).Days;
            var gracePeriod = payment.Tenant.LateFeeGracePeriodDays;
            var penaltyDays = Math.Max(0, daysOverdue - gracePeriod);
            var isWithinGracePeriod = daysOverdue <= gracePeriod;

            var lateFee = LateFeeCalculator.CalculateCurrentLateFee(payment.Tenant, payment.DueDate);

            var calculationDto = new LateFeeCalculationDto
            {
                PaymentId = payment.Id,
                Amount = payment.Amount,
                LateFeeAmount = lateFee,
                TotalAmount = payment.Amount + lateFee,
                DueDate = payment.DueDate,
                CurrentDate = currentDate,
                DaysOverdue = Math.Max(0, daysOverdue),
                GracePeriodDays = gracePeriod,
                PenaltyDays = penaltyDays,
                IsWithinGracePeriod = isWithinGracePeriod,
                LateFeePolicy = LateFeeCalculator.GetLateFeePolicy(payment.Tenant),
                LateFeeDetails = LateFeeCalculator.GetLateFeeDetails(payment.Tenant, payment.DueDate, currentDate)
            };

            return Result<LateFeeCalculationDto>.Success(calculationDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating late fee for payment {PaymentId}", paymentId);
            return Result<LateFeeCalculationDto>.Failure("An error occurred while calculating the late fee");
        }
    }

    private async Task<HashSet<int>> GetAssignedPropertyIdsAsync()
    {
        var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
        return assignedPropertyIds.Count == 0
            ? new HashSet<int>()
            : new HashSet<int>(assignedPropertyIds);
    }

    private bool IsInOrganizationScope(Property? property)
    {
        if (_currentUserService.IsPlatformAdmin)
        {
            return true;
        }

        return property != null &&
               _currentUserService.OrganizationId.HasValue &&
               property.OrganizationId == _currentUserService.OrganizationId.Value;
    }
}

