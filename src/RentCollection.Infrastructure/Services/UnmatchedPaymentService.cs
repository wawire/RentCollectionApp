using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Payments;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Enums;
using RentCollection.Application.Helpers;
using RentCollection.Domain.Entities;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Services;

public class UnmatchedPaymentService : IUnmatchedPaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentAllocationService _paymentAllocationService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<UnmatchedPaymentService> _logger;

    public UnmatchedPaymentService(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        IPaymentAllocationService paymentAllocationService,
        IAuditLogService auditLogService,
        ILogger<UnmatchedPaymentService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _paymentAllocationService = paymentAllocationService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<UnmatchedPaymentDto>>> GetUnmatchedPaymentsAsync(UnmatchedPaymentStatus? status = null)
    {
        try
        {
            if (!_currentUserService.IsPlatformAdmin && !_currentUserService.IsAccountant && !_currentUserService.IsLandlord && !_currentUserService.IsManager)
            {
                return Result<IEnumerable<UnmatchedPaymentDto>>.Failure("You do not have permission to view unmatched payments");
            }

            var query = _context.UnmatchedPayments.AsQueryable();

            if (!_currentUserService.IsPlatformAdmin)
            {
                if (!_currentUserService.OrganizationId.HasValue)
                {
                    return Result<IEnumerable<UnmatchedPaymentDto>>.Failure("You do not have permission to view unmatched payments");
                }

                var organizationId = _currentUserService.OrganizationId.Value;
                query = query.Where(p =>
                    (p.PropertyId.HasValue &&
                     _context.Properties.Any(pr => pr.Id == p.PropertyId.Value && pr.OrganizationId == organizationId)) ||
                    (!p.PropertyId.HasValue && p.LandlordId.HasValue &&
                     _context.Users.Any(u => u.Id == p.LandlordId.Value && u.OrganizationId == organizationId)));

                if (_currentUserService.IsLandlord)
                {
                    if (!_currentUserService.UserIdInt.HasValue)
                    {
                        return Result<IEnumerable<UnmatchedPaymentDto>>.Failure("You do not have permission to view unmatched payments");
                    }

                    query = query.Where(p => p.LandlordId == _currentUserService.UserIdInt.Value);
                }
                else
                {
                    var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
                    if (assignedPropertyIds.Count == 0)
                    {
                        return Result<IEnumerable<UnmatchedPaymentDto>>.Failure("You do not have permission to view unmatched payments");
                    }

                    query = query.Where(p => p.PropertyId.HasValue && assignedPropertyIds.Contains(p.PropertyId.Value));
                }
            }

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new UnmatchedPaymentDto
                {
                    Id = p.Id,
                    TransactionReference = p.TransactionReference,
                    Amount = p.Amount,
                    AccountReference = p.AccountReference,
                    PhoneNumber = p.PhoneNumber,
                    BusinessShortCode = p.BusinessShortCode,
                    CorrelationId = p.CorrelationId,
                    Reason = p.Reason,
                    LandlordId = p.LandlordId,
                    PropertyId = p.PropertyId,
                    ResolvedPaymentId = p.ResolvedPaymentId,
                    ResolvedByUserId = p.ResolvedByUserId,
                    ResolvedAt = p.ResolvedAt,
                    ResolutionNotes = p.ResolutionNotes,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return Result<IEnumerable<UnmatchedPaymentDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unmatched payments");
            return Result<IEnumerable<UnmatchedPaymentDto>>.Failure("An error occurred while retrieving unmatched payments");
        }
    }

    public async Task<Result<UnmatchedPaymentDto>> UpdateStatusAsync(int id, UnmatchedPaymentStatus status)
    {
        try
        {
            if (!_currentUserService.IsPlatformAdmin && !_currentUserService.IsLandlord && !_currentUserService.IsManager && !_currentUserService.IsAccountant)
            {
                return Result<UnmatchedPaymentDto>.Failure("You do not have permission to update unmatched payments");
            }

            var entity = await _context.UnmatchedPayments.FirstOrDefaultAsync(p => p.Id == id);
            if (entity == null)
            {
                return Result<UnmatchedPaymentDto>.Failure($"Unmatched payment with ID {id} not found");
            }

            if (!_currentUserService.IsPlatformAdmin)
            {
                if (!await IsInOrganizationScopeAsync(entity))
                {
                    return Result<UnmatchedPaymentDto>.Failure("You do not have permission to update unmatched payments");
                }

                if (_currentUserService.IsLandlord)
                {
                    if (!_currentUserService.UserIdInt.HasValue || entity.LandlordId != _currentUserService.UserIdInt.Value)
                    {
                        return Result<UnmatchedPaymentDto>.Failure("You do not have permission to update unmatched payments");
                    }
                }
                else
                {
                    var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
                    if (!entity.PropertyId.HasValue || !assignedPropertyIds.Contains(entity.PropertyId.Value))
                    {
                        return Result<UnmatchedPaymentDto>.Failure("You do not have permission to update unmatched payments");
                    }
                }
            }

            entity.Status = status;
            await _context.SaveChangesAsync();

            var dto = new UnmatchedPaymentDto
            {
                Id = entity.Id,
                TransactionReference = entity.TransactionReference,
                Amount = entity.Amount,
                AccountReference = entity.AccountReference,
                PhoneNumber = entity.PhoneNumber,
                BusinessShortCode = entity.BusinessShortCode,
                CorrelationId = entity.CorrelationId,
                Reason = entity.Reason,
                LandlordId = entity.LandlordId,
                PropertyId = entity.PropertyId,
                ResolvedPaymentId = entity.ResolvedPaymentId,
                ResolvedByUserId = entity.ResolvedByUserId,
                ResolvedAt = entity.ResolvedAt,
                ResolutionNotes = entity.ResolutionNotes,
                Status = entity.Status,
                CreatedAt = entity.CreatedAt
            };

            return Result<UnmatchedPaymentDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating unmatched payment {Id}", id);
            return Result<UnmatchedPaymentDto>.Failure("An error occurred while updating the unmatched payment");
        }
    }

    public async Task<Result<UnmatchedPaymentDto>> ResolveUnmatchedPaymentAsync(int id, ResolveUnmatchedPaymentDto dto)
    {
        try
        {
            if (!_currentUserService.IsPlatformAdmin && !_currentUserService.IsLandlord && !_currentUserService.IsManager && !_currentUserService.IsAccountant)
            {
                return Result<UnmatchedPaymentDto>.Failure("You do not have permission to resolve unmatched payments");
            }

            var unmatched = await _context.UnmatchedPayments.FirstOrDefaultAsync(p => p.Id == id);
            if (unmatched == null)
            {
                return Result<UnmatchedPaymentDto>.Failure($"Unmatched payment with ID {id} not found");
            }

            if (unmatched.Status == UnmatchedPaymentStatus.Resolved)
            {
                return Result<UnmatchedPaymentDto>.Failure("Unmatched payment is already resolved");
            }

            if (!_currentUserService.IsPlatformAdmin)
            {
                if (!await IsInOrganizationScopeAsync(unmatched))
                {
                    return Result<UnmatchedPaymentDto>.Failure("You do not have permission to resolve this unmatched payment");
                }

                if (_currentUserService.IsLandlord)
                {
                    if (!_currentUserService.UserIdInt.HasValue || (unmatched.LandlordId.HasValue && unmatched.LandlordId != _currentUserService.UserIdInt.Value))
                    {
                        return Result<UnmatchedPaymentDto>.Failure("You do not have permission to resolve this unmatched payment");
                    }
                }
                else
                {
                    var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
                    if (!unmatched.PropertyId.HasValue || !assignedPropertyIds.Contains(unmatched.PropertyId.Value))
                    {
                        return Result<UnmatchedPaymentDto>.Failure("You do not have permission to resolve this unmatched payment");
                    }
                }
            }

            var tenant = await _context.Tenants
                .Include(t => t.Unit)
                    .ThenInclude(u => u.Property)
                        .ThenInclude(p => p.PaymentAccounts)
                .FirstOrDefaultAsync(t => t.Id == dto.TenantId);

            if (tenant == null)
            {
                return Result<UnmatchedPaymentDto>.Failure($"Tenant with ID {dto.TenantId} not found");
            }

            if (!_currentUserService.IsPlatformAdmin)
            {
                if (!IsInOrganizationScope(tenant.Unit?.Property))
                {
                    return Result<UnmatchedPaymentDto>.Failure("You do not have permission to resolve payments for this tenant");
                }

                if (_currentUserService.IsLandlord)
                {
                    if (!_currentUserService.UserIdInt.HasValue || tenant.Unit?.Property?.LandlordId != _currentUserService.UserIdInt.Value)
                    {
                        return Result<UnmatchedPaymentDto>.Failure("You do not have permission to resolve payments for this tenant");
                    }
                }
                else
                {
                    var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
                    if (tenant.Unit == null || !assignedPropertyIds.Contains(tenant.Unit.PropertyId))
                    {
                        return Result<UnmatchedPaymentDto>.Failure("You do not have permission to resolve payments for this tenant");
                    }
                }
            }

            if (dto.PeriodEnd <= dto.PeriodStart)
            {
                return Result<UnmatchedPaymentDto>.Failure("Period end date must be after period start date");
            }

            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.TransactionReference == unmatched.TransactionReference);

            if (existingPayment != null)
            {
                return Result<UnmatchedPaymentDto>.Failure("A payment with this transaction reference already exists");
            }

            if (tenant.Unit == null)
            {
                return Result<UnmatchedPaymentDto>.Failure("Tenant does not have an assigned unit");
            }

            var paymentAccount = tenant.Unit.Property.PaymentAccounts
                .FirstOrDefault(pa => pa.Id == dto.LandlordAccountId && pa.IsActive) ??
                tenant.Unit.Property.PaymentAccounts.FirstOrDefault(pa => pa.IsDefault && pa.IsActive) ??
                tenant.Unit.Property.PaymentAccounts.FirstOrDefault(pa => pa.IsActive);

            if (paymentAccount == null)
            {
                return Result<UnmatchedPaymentDto>.Failure("No active payment account configured for this property");
            }

            var dueDate = PaymentDueDateHelper.CalculateDueDateForMonth(
                dto.PeriodStart.Year,
                dto.PeriodStart.Month,
                tenant.RentDueDay);

            var payment = new Payment
            {
                TenantId = tenant.Id,
                UnitId = tenant.UnitId,
                LandlordAccountId = paymentAccount.Id,
                Amount = unmatched.Amount,
                PaymentDate = dto.PaymentDate ?? DateTime.UtcNow,
                DueDate = dueDate,
                PaymentMethod = PaymentMethod.MPesa,
                Status = PaymentStatus.Completed,
                UnallocatedAmount = unmatched.Amount,
                TransactionReference = unmatched.TransactionReference,
                PaybillAccountNumber = unmatched.AccountReference,
                MPesaPhoneNumber = unmatched.PhoneNumber,
                Notes = dto.Notes,
                PeriodStart = dto.PeriodStart,
                PeriodEnd = dto.PeriodEnd,
                ConfirmedAt = DateTime.UtcNow,
                ConfirmedByUserId = _currentUserService.UserIdInt
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            var allocationResult = await _paymentAllocationService.AllocatePaymentToOutstandingInvoicesAsync(payment.Id);
            if (!allocationResult.IsSuccess)
            {
                _logger.LogWarning("Auto-allocation failed for payment {PaymentId}: {Error}", payment.Id, allocationResult.ErrorMessage);
            }

            unmatched.Status = UnmatchedPaymentStatus.Resolved;
            unmatched.ResolvedPaymentId = payment.Id;
            unmatched.ResolvedAt = DateTime.UtcNow;
            unmatched.ResolvedByUserId = _currentUserService.UserIdInt;
            unmatched.ResolutionNotes = dto.Notes;

            await _context.SaveChangesAsync();

            await _auditLogService.LogActionAsync(
                "Resolve",
                "UnmatchedPayment",
                unmatched.Id,
                $"Resolved unmatched payment {unmatched.TransactionReference} for Tenant#{dto.TenantId}");

            var dtoResult = new UnmatchedPaymentDto
            {
                Id = unmatched.Id,
                TransactionReference = unmatched.TransactionReference,
                Amount = unmatched.Amount,
                AccountReference = unmatched.AccountReference,
                PhoneNumber = unmatched.PhoneNumber,
                BusinessShortCode = unmatched.BusinessShortCode,
                CorrelationId = unmatched.CorrelationId,
                Reason = unmatched.Reason,
                LandlordId = unmatched.LandlordId,
                PropertyId = unmatched.PropertyId,
                ResolvedPaymentId = unmatched.ResolvedPaymentId,
                ResolvedByUserId = unmatched.ResolvedByUserId,
                ResolvedAt = unmatched.ResolvedAt,
                ResolutionNotes = unmatched.ResolutionNotes,
                Status = unmatched.Status,
                CreatedAt = unmatched.CreatedAt
            };

            return Result<UnmatchedPaymentDto>.Success(dtoResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving unmatched payment {Id}", id);
            return Result<UnmatchedPaymentDto>.Failure("An error occurred while resolving the unmatched payment");
        }
    }

    private async Task<bool> IsInOrganizationScopeAsync(UnmatchedPayment payment)
    {
        if (_currentUserService.IsPlatformAdmin)
        {
            return true;
        }

        if (!_currentUserService.OrganizationId.HasValue)
        {
            return false;
        }

        var organizationId = _currentUserService.OrganizationId.Value;

        if (payment.PropertyId.HasValue)
        {
            return await _context.Properties
                .AnyAsync(p => p.Id == payment.PropertyId.Value && p.OrganizationId == organizationId);
        }

        if (payment.LandlordId.HasValue)
        {
            return await _context.Users
                .AnyAsync(u => u.Id == payment.LandlordId.Value && u.OrganizationId == organizationId);
        }

        return false;
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

