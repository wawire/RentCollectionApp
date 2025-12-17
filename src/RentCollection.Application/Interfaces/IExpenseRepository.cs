using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Interfaces;

public interface IExpenseRepository : IRepository<Expense>
{
    Task<IEnumerable<Expense>> GetExpensesByPropertyIdAsync(int propertyId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<Expense>> GetExpensesByLandlordIdAsync(int landlordId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<Expense>> GetExpensesByCategoryAsync(int landlordId, ExpenseCategory category, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<Expense>> GetRecurringExpensesAsync(int landlordId);
    Task<IEnumerable<Expense>> GetExpensesDueForRecurrenceAsync(DateTime date);
    Task<Expense?> GetExpenseWithDetailsAsync(int id);
    Task<decimal> GetTotalExpensesAsync(int landlordId, DateTime? startDate = null, DateTime? endDate = null);
    Task<decimal> GetTotalExpensesByPropertyAsync(int propertyId, DateTime? startDate = null, DateTime? endDate = null);
    Task<Dictionary<string, decimal>> GetExpensesByCategoryBreakdownAsync(int landlordId, DateTime? startDate = null, DateTime? endDate = null);
}
