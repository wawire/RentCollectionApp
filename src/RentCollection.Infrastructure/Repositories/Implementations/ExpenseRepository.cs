using Microsoft.EntityFrameworkCore;
using RentCollection.Application.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Repositories.Implementations;

public class ExpenseRepository : Repository<Expense>, IExpenseRepository
{
    public ExpenseRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Expense>> GetExpensesByPropertyIdAsync(int propertyId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Expenses
            .Include(e => e.Property)
            .Include(e => e.Unit)
            .Where(e => e.PropertyId == propertyId);

        if (startDate.HasValue)
            query = query.Where(e => e.ExpenseDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(e => e.ExpenseDate <= endDate.Value);

        return await query
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Expense>> GetExpensesByLandlordIdAsync(int landlordId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Expenses
            .Include(e => e.Property)
            .Include(e => e.Unit)
            .Where(e => e.LandlordId == landlordId);

        if (startDate.HasValue)
            query = query.Where(e => e.ExpenseDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(e => e.ExpenseDate <= endDate.Value);

        return await query
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Expense>> GetExpensesByCategoryAsync(int landlordId, ExpenseCategory category, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Expenses
            .Include(e => e.Property)
            .Include(e => e.Unit)
            .Where(e => e.LandlordId == landlordId && e.Category == category);

        if (startDate.HasValue)
            query = query.Where(e => e.ExpenseDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(e => e.ExpenseDate <= endDate.Value);

        return await query
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Expense>> GetRecurringExpensesAsync(int landlordId)
    {
        return await _context.Expenses
            .Include(e => e.Property)
            .Include(e => e.Unit)
            .Where(e => e.LandlordId == landlordId && e.IsRecurring)
            .OrderBy(e => e.NextRecurrenceDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Expense>> GetExpensesDueForRecurrenceAsync(DateTime date)
    {
        return await _context.Expenses
            .Include(e => e.Property)
            .Where(e => e.IsRecurring && e.NextRecurrenceDate <= date)
            .ToListAsync();
    }

    public async Task<Expense?> GetExpenseWithDetailsAsync(int id)
    {
        return await _context.Expenses
            .Include(e => e.Property)
            .Include(e => e.Unit)
            .Include(e => e.Landlord)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<decimal> GetTotalExpensesAsync(int landlordId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Expenses
            .Where(e => e.LandlordId == landlordId);

        if (startDate.HasValue)
            query = query.Where(e => e.ExpenseDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(e => e.ExpenseDate <= endDate.Value);

        return await query.SumAsync(e => e.Amount);
    }

    public async Task<decimal> GetTotalExpensesByPropertyAsync(int propertyId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Expenses
            .Where(e => e.PropertyId == propertyId);

        if (startDate.HasValue)
            query = query.Where(e => e.ExpenseDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(e => e.ExpenseDate <= endDate.Value);

        return await query.SumAsync(e => e.Amount);
    }

    public async Task<Dictionary<string, decimal>> GetExpensesByCategoryBreakdownAsync(int landlordId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Expenses
            .Where(e => e.LandlordId == landlordId);

        if (startDate.HasValue)
            query = query.Where(e => e.ExpenseDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(e => e.ExpenseDate <= endDate.Value);

        return await query
            .GroupBy(e => e.Category)
            .Select(g => new { Category = g.Key.ToString(), Total = g.Sum(e => e.Amount) })
            .ToDictionaryAsync(x => x.Category, x => x.Total);
    }
}
