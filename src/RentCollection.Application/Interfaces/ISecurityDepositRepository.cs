using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Interfaces;

public interface ISecurityDepositRepository : IRepository<SecurityDepositTransaction>
{
    Task<List<SecurityDepositTransaction>> GetByTenantIdAsync(int tenantId);
    Task<decimal> GetCurrentBalanceAsync(int tenantId);
    Task<SecurityDepositTransaction?> GetInitialDepositAsync(int tenantId);
    Task<List<SecurityDepositTransaction>> GetByTypeAsync(int tenantId, SecurityDepositTransactionType type);
    Task<List<SecurityDepositTransaction>> GetTransactionHistoryAsync(int tenantId, DateTime? startDate = null, DateTime? endDate = null);
}
