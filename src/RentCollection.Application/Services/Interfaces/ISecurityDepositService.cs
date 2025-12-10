using RentCollection.Application.Common;
using RentCollection.Application.DTOs.SecurityDeposits;

namespace RentCollection.Application.Services.Interfaces;

public interface ISecurityDepositService
{
    /// <summary>
    /// Record initial security deposit payment from tenant
    /// </summary>
    Task<ServiceResult<SecurityDepositTransactionDto>> RecordDepositPaymentAsync(int tenantId, RecordSecurityDepositDto dto, int userId);

    /// <summary>
    /// Deduct amount from security deposit (e.g., damages, unpaid rent)
    /// </summary>
    Task<ServiceResult<SecurityDepositTransactionDto>> DeductFromDepositAsync(int tenantId, DeductSecurityDepositDto dto, int userId);

    /// <summary>
    /// Refund security deposit to tenant (full or partial)
    /// </summary>
    Task<ServiceResult<SecurityDepositTransactionDto>> RefundDepositAsync(int tenantId, RefundSecurityDepositDto dto, int userId);

    /// <summary>
    /// Get current security deposit balance for tenant
    /// </summary>
    Task<ServiceResult<SecurityDepositBalanceDto>> GetDepositBalanceAsync(int tenantId);

    /// <summary>
    /// Get transaction history for tenant's security deposit
    /// </summary>
    Task<ServiceResult<List<SecurityDepositTransactionDto>>> GetTransactionHistoryAsync(int tenantId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Get all tenants with security deposit balances
    /// </summary>
    Task<ServiceResult<List<SecurityDepositBalanceDto>>> GetAllDepositsAsync();
}
