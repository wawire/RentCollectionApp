using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Payments;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Application.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PaymentService> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _context;

    public PaymentService(
        IPaymentRepository paymentRepository,
        ITenantRepository tenantRepository,
        IMapper mapper,
        ILogger<PaymentService> logger,
        ICurrentUserService currentUserService,
        ApplicationDbContext context)
    {
        _paymentRepository = paymentRepository;
        _tenantRepository = tenantRepository;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
        _context = context;
    }

    public async Task<Result<IEnumerable<PaymentDto>>> GetAllPaymentsAsync()
    {
        try
        {
            var payments = await _paymentRepository.GetAllAsync();

            // Filter payments by tenant's unit's property's LandlordId (unless SystemAdmin)
            if (!_currentUserService.IsSystemAdmin)
            {
                var landlordIdStr = _currentUserService.IsLandlord
                    ? _currentUserService.UserId
                    : _currentUserService.LandlordId;

                if (int.TryParse(landlordIdStr, out var landlordId))
                {
                    payments = payments.Where(p => p.Tenant?.Unit?.Property?.LandlordId == landlordId).ToList();
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
            if (!_currentUserService.IsSystemAdmin)
            {
                var landlordIdStr = _currentUserService.IsLandlord
                    ? _currentUserService.UserId
                    : _currentUserService.LandlordId;

                if (int.TryParse(landlordIdStr, out var landlordId))
                {
                    if (tenant.Unit?.Property?.LandlordId != landlordId)
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
            if (!_currentUserService.IsSystemAdmin)
            {
                var landlordIdStr = _currentUserService.IsLandlord
                    ? _currentUserService.UserId
                    : _currentUserService.LandlordId;

                if (int.TryParse(landlordIdStr, out var landlordId))
                {
                    if (payment.Tenant?.Unit?.Property?.LandlordId != landlordId)
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
            var tenant = await _tenantRepository.GetTenantWithDetailsAsync(createDto.TenantId);
            if (tenant == null)
            {
                return Result<PaymentDto>.Failure($"Tenant with ID {createDto.TenantId} not found");
            }

            // Check access permission - user must have access to the tenant's unit's property
            if (!_currentUserService.IsSystemAdmin)
            {
                var landlordIdStr = _currentUserService.IsLandlord
                    ? _currentUserService.UserId
                    : _currentUserService.LandlordId;

                if (int.TryParse(landlordIdStr, out var landlordId))
                {
                    if (tenant.Unit?.Property?.LandlordId != landlordId)
                    {
                        return Result<PaymentDto>.Failure("You do not have permission to record payments for this tenant");
                    }
                }

                // Accountants cannot record payments (read-only access)
                if (_currentUserService.IsAccountant)
                {
                    return Result<PaymentDto>.Failure("Accountants do not have permission to record payments");
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

            var payment = _mapper.Map<Payment>(createDto);
            payment.CreatedAt = DateTime.UtcNow;
            payment.Status = PaymentStatus.Completed; // Set as completed by default

            var createdPayment = await _paymentRepository.AddAsync(payment);

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

            // Check access permission - Only SystemAdmin and Landlords can delete
            if (!_currentUserService.IsSystemAdmin && !_currentUserService.IsLandlord)
            {
                return Result.Failure("You do not have permission to delete payments");
            }

            if (!_currentUserService.IsSystemAdmin)
            {
                var landlordIdStr = _currentUserService.UserId; // Must be landlord at this point

                if (int.TryParse(landlordIdStr, out var landlordId))
                {
                    if (payment.Tenant?.Unit?.Property?.LandlordId != landlordId)
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

            // Filter payments by tenant's unit's property's LandlordId (unless SystemAdmin)
            if (!_currentUserService.IsSystemAdmin)
            {
                var landlordIdStr = _currentUserService.IsLandlord
                    ? _currentUserService.UserId
                    : _currentUserService.LandlordId;

                if (int.TryParse(landlordIdStr, out var landlordId))
                {
                    allPayments = allPayments.Where(p => p.Tenant?.Unit?.Property?.LandlordId == landlordId).ToList();
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
            var payment = new Payment
            {
                TenantId = tenantId,
                UnitId = tenant.Unit.Id,
                LandlordAccountId = paymentAccount.Id,
                Amount = dto.Amount,
                PaymentDate = dto.PaymentDate,
                PaymentMethod = dto.PaymentMethod,
                Status = PaymentStatus.Pending, // Pending until landlord confirms
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

    public async Task<Result<IEnumerable<PaymentDto>>> GetPendingPaymentsAsync(int landlordId, int? propertyId = null)
    {
        try
        {
            var query = _context.Payments
                .Include(p => p.Tenant)
                .Include(p => p.Unit)
                .Include(p => p.LandlordAccount)
                .Where(p => p.Status == PaymentStatus.Pending);

            // Filter by landlord
            query = query.Where(p => p.Unit.Property.LandlordId == landlordId);

            // Filter by property if specified
            if (propertyId.HasValue)
            {
                query = query.Where(p => p.Unit.PropertyId == propertyId.Value);
            }

            var payments = await query
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            var paymentDtos = _mapper.Map<IEnumerable<PaymentDto>>(payments);

            return Result<IEnumerable<PaymentDto>>.Success(paymentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending payments for landlord {LandlordId}", landlordId);
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
                .Include(p => p.LandlordAccount)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return Result<PaymentDto>.Failure($"Payment with ID {paymentId} not found");
            }

            if (payment.Status != PaymentStatus.Pending)
            {
                return Result<PaymentDto>.Failure($"Payment is already {payment.Status}. Only pending payments can be confirmed.");
            }

            // Update payment status
            payment.Status = PaymentStatus.Completed;
            payment.ConfirmedAt = DateTime.UtcNow;
            payment.ConfirmedByUserId = confirmedByUserId;
            payment.UpdatedAt = DateTime.UtcNow;

            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();

            var paymentDto = _mapper.Map<PaymentDto>(payment);

            _logger.LogInformation("Payment {PaymentId} confirmed by user {UserId}", paymentId, confirmedByUserId);

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
                .Include(p => p.LandlordAccount)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return Result<PaymentDto>.Failure($"Payment with ID {paymentId} not found");
            }

            if (payment.Status != PaymentStatus.Pending)
            {
                return Result<PaymentDto>.Failure($"Payment is already {payment.Status}. Only pending payments can be rejected.");
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

            return Result<PaymentDto>.Success(paymentDto, "Payment rejected");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting payment {PaymentId}", paymentId);
            return Result<PaymentDto>.Failure("An error occurred while rejecting the payment");
        }
    }
}
