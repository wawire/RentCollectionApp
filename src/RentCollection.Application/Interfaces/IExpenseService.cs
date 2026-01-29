using RentCollection.Application.DTOs.Expenses;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Interfaces
{
    public interface IExpenseService
    {
        // CRUD Operations
        Task<ExpenseDto> GetExpenseByIdAsync(int id);
        Task<List<ExpenseDto>> GetAllExpensesAsync(int landlordId);
        Task<List<ExpenseDto>> GetExpensesByPropertyAsync(int propertyId, DateTime? startDate = null, DateTime? endDate = null);
        Task<List<ExpenseDto>> GetExpensesByLandlordAsync(int landlordId, DateTime? startDate = null, DateTime? endDate = null);
        Task<List<ExpenseDto>> GetExpensesByCategoryAsync(int landlordId, ExpenseCategory category, DateTime? startDate = null, DateTime? endDate = null);
        Task<List<ExpenseDto>> GetRecurringExpensesAsync(int landlordId);
        Task<ExpenseDto> CreateExpenseAsync(CreateExpenseDto dto);
        Task<ExpenseDto> UpdateExpenseAsync(int id, UpdateExpenseDto dto);
        Task<bool> DeleteExpenseAsync(int id);

        // Statistics and Analytics
        Task<ExpenseSummaryDto> GetExpenseSummaryAsync(int landlordId, DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetTotalExpensesAsync(int landlordId, DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetTotalExpensesByPropertyAsync(int propertyId, DateTime? startDate = null, DateTime? endDate = null);

        // Recurring Expenses
        Task ProcessRecurringExpensesAsync();
    }
}
