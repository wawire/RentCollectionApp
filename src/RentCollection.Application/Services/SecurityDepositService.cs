using Microsoft.Extensions.Logging;
using RentCollection.Application.Common;
using RentCollection.Application.DTOs.SecurityDeposits;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Services;

public class SecurityDepositService : ISecurityDepositService
{
    private readonly ISecurityDepositRepository _depositRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMaintenanceRequestRepository _maintenanceRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<SecurityDepositService> _logger;

    public SecurityDepositService(
        ISecurityDepositRepository depositRepository,
        ITenantRepository tenantRepository,
        IPaymentRepository paymentRepository,
        IMaintenanceRequestRepository maintenanceRepository,
        IAuditLogService auditLogService,
        ILogger<SecurityDepositService> logger)
    {
        _depositRepository = depositRepository;
        _tenantRepository = tenantRepository;
        _paymentRepository = paymentRepository;
        _maintenanceRepository = maintenanceRepository;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<ServiceResult<SecurityDepositTransactionDto>> RecordDepositPaymentAsync(
        int tenantId,
        RecordSecurityDepositDto dto,
        int userId)
    {
        try
        {
            // Validate tenant exists
            var tenant = await _tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
                return ServiceResult<SecurityDepositTransactionDto>.Failure("Tenant not found");

            // Validate related payment if provided
            if (dto.RelatedPaymentId.HasValue)
            {
                var payment = await _paymentRepository.GetByIdAsync(dto.RelatedPaymentId.Value);
                if (payment == null)
                    return ServiceResult<SecurityDepositTransactionDto>.Failure("Related payment not found");

                if (payment.TenantId != tenantId)
                    return ServiceResult<SecurityDepositTransactionDto>.Failure("Payment does not belong to this tenant");
            }

            var transaction = new SecurityDepositTransaction
            {
                TenantId = tenantId,
                Amount = dto.Amount,
                TransactionType = SecurityDepositTransactionType.Initial,
                TransactionDate = dto.TransactionDate,
                RelatedPaymentId = dto.RelatedPaymentId,
                Notes = dto.Notes,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _depositRepository.AddAsync(transaction);

            // Audit log
            await _auditLogService.LogAsync(
                userId,
                "SecurityDeposit",
                transaction.Id,
                AuditAction.Create,
                $"Recorded security deposit of {dto.Amount:C} for tenant {tenant.Name}"
            );

            _logger.LogInformation("Security deposit of {Amount} recorded for tenant {TenantId} by user {UserId}",
                dto.Amount, tenantId, userId);

            return ServiceResult<SecurityDepositTransactionDto>.Success(MapToDto(transaction));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording security deposit for tenant {TenantId}", tenantId);
            return ServiceResult<SecurityDepositTransactionDto>.Failure($"Error recording deposit: {ex.Message}");
        }
    }

    public async Task<ServiceResult<SecurityDepositTransactionDto>> DeductFromDepositAsync(
        int tenantId,
        DeductSecurityDepositDto dto,
        int userId)
    {
        try
        {
            // Validate tenant exists
            var tenant = await _tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
                return ServiceResult<SecurityDepositTransactionDto>.Failure("Tenant not found");

            // Check current balance
            var currentBalance = await _depositRepository.GetCurrentBalanceAsync(tenantId);
            if (currentBalance < dto.Amount)
                return ServiceResult<SecurityDepositTransactionDto>.Failure(
                    $"Insufficient deposit balance. Current balance: {currentBalance:C}, Deduction amount: {dto.Amount:C}");

            // Validate related maintenance request if provided
            if (dto.RelatedMaintenanceRequestId.HasValue)
            {
                var maintenanceRequest = await _maintenanceRepository.GetByIdAsync(dto.RelatedMaintenanceRequestId.Value);
                if (maintenanceRequest == null)
                    return ServiceResult<SecurityDepositTransactionDto>.Failure("Related maintenance request not found");

                if (maintenanceRequest.TenantId != tenantId)
                    return ServiceResult<SecurityDepositTransactionDto>.Failure("Maintenance request does not belong to this tenant");
            }

            var transaction = new SecurityDepositTransaction
            {
                TenantId = tenantId,
                Amount = dto.Amount,
                TransactionType = SecurityDepositTransactionType.Deduction,
                Reason = dto.Reason,
                TransactionDate = DateTime.UtcNow,
                RelatedMaintenanceRequestId = dto.RelatedMaintenanceRequestId,
                Notes = dto.Notes,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _depositRepository.AddAsync(transaction);

            // Audit log
            await _auditLogService.LogAsync(
                userId,
                "SecurityDeposit",
                transaction.Id,
                AuditAction.Update,
                $"Deducted {dto.Amount:C} from security deposit for tenant {tenant.Name}. Reason: {dto.Reason}"
            );

            _logger.LogInformation("Deducted {Amount} from security deposit for tenant {TenantId}. Reason: {Reason}",
                dto.Amount, tenantId, dto.Reason);

            return ServiceResult<SecurityDepositTransactionDto>.Success(MapToDto(transaction));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deducting from security deposit for tenant {TenantId}", tenantId);
            return ServiceResult<SecurityDepositTransactionDto>.Failure($"Error processing deduction: {ex.Message}");
        }
    }

    public async Task<ServiceResult<SecurityDepositTransactionDto>> RefundDepositAsync(
        int tenantId,
        RefundSecurityDepositDto dto,
        int userId)
    {
        try
        {
            // Validate tenant exists
            var tenant = await _tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
                return ServiceResult<SecurityDepositTransactionDto>.Failure("Tenant not found");

            // Check current balance
            var currentBalance = await _depositRepository.GetCurrentBalanceAsync(tenantId);
            if (currentBalance < dto.Amount)
                return ServiceResult<SecurityDepositTransactionDto>.Failure(
                    $"Refund amount exceeds available balance. Current balance: {currentBalance:C}, Refund amount: {dto.Amount:C}");

            var transaction = new SecurityDepositTransaction
            {
                TenantId = tenantId,
                Amount = dto.Amount,
                TransactionType = SecurityDepositTransactionType.Refund,
                Reason = $"Refund via {dto.RefundMethod}",
                TransactionDate = dto.RefundDate,
                Notes = $"Transaction Ref: {dto.TransactionReference}. {dto.Notes}",
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _depositRepository.AddAsync(transaction);

            // Audit log
            await _auditLogService.LogAsync(
                userId,
                "SecurityDeposit",
                transaction.Id,
                AuditAction.Update,
                $"Refunded {dto.Amount:C} security deposit to tenant {tenant.Name} via {dto.RefundMethod}"
            );

            _logger.LogInformation("Refunded {Amount} security deposit to tenant {TenantId} via {Method}",
                dto.Amount, tenantId, dto.RefundMethod);

            return ServiceResult<SecurityDepositTransactionDto>.Success(MapToDto(transaction));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding security deposit for tenant {TenantId}", tenantId);
            return ServiceResult<SecurityDepositTransactionDto>.Failure($"Error processing refund: {ex.Message}");
        }
    }

    public async Task<ServiceResult<SecurityDepositBalanceDto>> GetDepositBalanceAsync(int tenantId)
    {
        try
        {
            var tenant = await _tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
                return ServiceResult<SecurityDepositBalanceDto>.Failure("Tenant not found");

            var transactions = await _depositRepository.GetByTenantIdAsync(tenantId);

            var initialDeposit = transactions
                .Where(t => t.TransactionType == SecurityDepositTransactionType.Initial)
                .Sum(t => t.Amount);

            var totalDeductions = transactions
                .Where(t => t.TransactionType == SecurityDepositTransactionType.Deduction)
                .Sum(t => t.Amount);

            var totalRefunds = transactions
                .Where(t => t.TransactionType == SecurityDepositTransactionType.Refund)
                .Sum(t => t.Amount);

            var currentBalance = initialDeposit - totalDeductions - totalRefunds;

            var balanceDto = new SecurityDepositBalanceDto
            {
                TenantId = tenantId,
                TenantName = tenant.Name,
                UnitNumber = tenant.UnitNumber,
                InitialDeposit = initialDeposit,
                TotalDeductions = totalDeductions,
                TotalRefunds = totalRefunds,
                CurrentBalance = currentBalance,
                LastTransactionDate = transactions.FirstOrDefault()?.TransactionDate,
                TotalTransactions = transactions.Count,
                RecentTransactions = transactions.Take(5).Select(MapToDto).ToList()
            };

            return ServiceResult<SecurityDepositBalanceDto>.Success(balanceDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting deposit balance for tenant {TenantId}", tenantId);
            return ServiceResult<SecurityDepositBalanceDto>.Failure($"Error retrieving balance: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<SecurityDepositTransactionDto>>> GetTransactionHistoryAsync(
        int tenantId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            var tenant = await _tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
                return ServiceResult<List<SecurityDepositTransactionDto>>.Failure("Tenant not found");

            var transactions = await _depositRepository.GetTransactionHistoryAsync(tenantId, startDate, endDate);
            var dtos = transactions.Select(MapToDto).ToList();

            return ServiceResult<List<SecurityDepositTransactionDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction history for tenant {TenantId}", tenantId);
            return ServiceResult<List<SecurityDepositTransactionDto>>.Failure($"Error retrieving history: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<SecurityDepositBalanceDto>>> GetAllDepositsAsync()
    {
        try
        {
            var tenants = await _tenantRepository.GetAllAsync();
            var balances = new List<SecurityDepositBalanceDto>();

            foreach (var tenant in tenants)
            {
                var result = await GetDepositBalanceAsync(tenant.Id);
                if (result.IsSuccess && result.Data != null)
                {
                    balances.Add(result.Data);
                }
            }

            // Filter to only show tenants with deposits
            var tenantsWithDeposits = balances.Where(b => b.InitialDeposit > 0).ToList();

            return ServiceResult<List<SecurityDepositBalanceDto>>.Success(tenantsWithDeposits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all security deposits");
            return ServiceResult<List<SecurityDepositBalanceDto>>.Failure($"Error retrieving deposits: {ex.Message}");
        }
    }

    private SecurityDepositTransactionDto MapToDto(SecurityDepositTransaction transaction)
    {
        return new SecurityDepositTransactionDto
        {
            Id = transaction.Id,
            TenantId = transaction.TenantId,
            TenantName = transaction.Tenant?.Name ?? "Unknown",
            UnitNumber = transaction.Tenant?.UnitNumber ?? "N/A",
            Amount = transaction.Amount,
            TransactionType = transaction.TransactionType,
            TransactionTypeDisplay = transaction.TransactionType.ToString(),
            Reason = transaction.Reason,
            TransactionDate = transaction.TransactionDate,
            RelatedPaymentId = transaction.RelatedPaymentId,
            RelatedMaintenanceRequestId = transaction.RelatedMaintenanceRequestId,
            ReceiptUrl = transaction.ReceiptUrl,
            Notes = transaction.Notes,
            CreatedByUserId = transaction.CreatedByUserId,
            CreatedByUserName = transaction.CreatedByUser?.Name ?? "Unknown",
            CreatedAt = transaction.CreatedAt
        };
    }
}
