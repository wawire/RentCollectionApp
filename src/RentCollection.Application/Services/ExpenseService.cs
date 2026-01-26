using Microsoft.Extensions.Logging;
using RentCollection.Application.DTOs.Expenses;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly ILogger<ExpenseService> _logger;
        private readonly ICurrentUserService _currentUserService;

        public ExpenseService(
            IExpenseRepository expenseRepository,
            IPropertyRepository propertyRepository,
            ICurrentUserService currentUserService,
            ILogger<ExpenseService> logger)
        {
            _expenseRepository = expenseRepository;
            _propertyRepository = propertyRepository;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ExpenseDto> GetExpenseByIdAsync(int id)
        {
            var expense = await _expenseRepository.GetExpenseWithDetailsAsync(id);
            if (expense == null)
            {
                throw new InvalidOperationException($"Expense {id} not found");
            }

            if (!CanAccessExpense(expense))
            {
                throw new UnauthorizedAccessException("You do not have permission to view this expense");
            }

            return MapToDto(expense);
        }

        public async Task<List<ExpenseDto>> GetAllExpensesAsync(int landlordId)
        {
            if (!CanAccessLandlord(landlordId))
            {
                throw new UnauthorizedAccessException("You do not have permission to view these expenses");
            }

            var expenses = await _expenseRepository.GetExpensesByLandlordIdAsync(landlordId);
            return expenses.Select(MapToDto).ToList();
        }

        public async Task<List<ExpenseDto>> GetExpensesByPropertyAsync(int propertyId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!await CanAccessPropertyAsync(propertyId))
            {
                throw new UnauthorizedAccessException("You do not have permission to view expenses for this property");
            }

            var expenses = await _expenseRepository.GetExpensesByPropertyIdAsync(propertyId, startDate, endDate);
            return expenses.Select(MapToDto).ToList();
        }

        public async Task<List<ExpenseDto>> GetExpensesByLandlordAsync(int landlordId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!CanAccessLandlord(landlordId))
            {
                throw new UnauthorizedAccessException("You do not have permission to view these expenses");
            }

            var expenses = await _expenseRepository.GetExpensesByLandlordIdAsync(landlordId, startDate, endDate);
            return expenses.Select(MapToDto).ToList();
        }

        public async Task<List<ExpenseDto>> GetExpensesByCategoryAsync(int landlordId, ExpenseCategory category, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!CanAccessLandlord(landlordId))
            {
                throw new UnauthorizedAccessException("You do not have permission to view these expenses");
            }

            var expenses = await _expenseRepository.GetExpensesByCategoryAsync(landlordId, category, startDate, endDate);
            return expenses.Select(MapToDto).ToList();
        }

        public async Task<List<ExpenseDto>> GetRecurringExpensesAsync(int landlordId)
        {
            if (!CanAccessLandlord(landlordId))
            {
                throw new UnauthorizedAccessException("You do not have permission to view these expenses");
            }

            var expenses = await _expenseRepository.GetRecurringExpensesAsync(landlordId);
            return expenses.Select(MapToDto).ToList();
        }

        public async Task<ExpenseDto> CreateExpenseAsync(CreateExpenseDto dto)
        {
            if (_currentUserService.IsCaretaker)
            {
                throw new UnauthorizedAccessException("You do not have permission to add expenses");
            }

            var property = await _propertyRepository.GetByIdAsync(dto.PropertyId);
            if (property == null)
            {
                throw new InvalidOperationException($"Property {dto.PropertyId} not found");
            }

            if (!property.LandlordId.HasValue)
            {
                throw new InvalidOperationException("Property does not have an owner");
            }

            if (!_currentUserService.IsPlatformAdmin)
            {
                if (_currentUserService.IsLandlord)
                {
                    if (!_currentUserService.UserIdInt.HasValue || property.LandlordId != _currentUserService.UserIdInt.Value)
                    {
                        throw new UnauthorizedAccessException("You do not have permission to add expenses to this property");
                    }
                }
                else if (_currentUserService.IsManager || _currentUserService.IsAccountant)
                {
                var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
                    if (!assignedPropertyIds.Contains(property.Id))
                    {
                        throw new UnauthorizedAccessException("You do not have permission to add expenses to this property");
                    }
                }
                else
                {
                    throw new UnauthorizedAccessException("You do not have permission to add expenses");
                }
            }

            var expense = new Expense
            {
                PropertyId = dto.PropertyId,
                LandlordId = property.LandlordId.Value,
                UnitId = dto.UnitId,
                Category = dto.Category,
                Amount = dto.Amount,
                ExpenseDate = dto.ExpenseDate,
                Vendor = dto.Vendor,
                Description = dto.Description,
                PaymentMethod = dto.PaymentMethod,
                ReferenceNumber = dto.ReferenceNumber,
                IsRecurring = dto.IsRecurring,
                RecurrenceMonths = dto.RecurrenceMonths,
                ReceiptUrl = dto.ReceiptUrl,
                Notes = dto.Notes,
                IsTaxDeductible = dto.IsTaxDeductible,
                Tags = dto.Tags
            };

            // Set next recurrence date if recurring
            if (expense.IsRecurring && expense.RecurrenceMonths.HasValue)
            {
                expense.NextRecurrenceDate = expense.ExpenseDate.AddMonths(expense.RecurrenceMonths.Value);
            }

            var created = await _expenseRepository.AddAsync(expense);

            _logger.LogInformation("Created expense {ExpenseId} for property {PropertyId} by landlord {LandlordId}",
                created.Id, dto.PropertyId, property.LandlordId.Value);

            // Reload with navigation properties
            var result = await _expenseRepository.GetExpenseWithDetailsAsync(created.Id);
            return MapToDto(result!);
        }

        public async Task<ExpenseDto> UpdateExpenseAsync(int id, UpdateExpenseDto dto)
        {
            var expense = await _expenseRepository.GetExpenseWithDetailsAsync(id);
            if (expense == null)
            {
                throw new InvalidOperationException($"Expense {id} not found");
            }

            if (!CanAccessExpense(expense))
            {
                throw new UnauthorizedAccessException("You do not have permission to update this expense");
            }

            if (dto.PropertyId != expense.PropertyId)
            {
                var property = await _propertyRepository.GetByIdAsync(dto.PropertyId);
                if (property == null)
                {
                    throw new InvalidOperationException($"Property {dto.PropertyId} not found");
                }

                if (!property.LandlordId.HasValue || !CanAccessLandlord(property.LandlordId.Value))
                {
                    throw new UnauthorizedAccessException("You do not have permission to assign expenses to this property");
                }
            }

            // Update properties
            expense.PropertyId = dto.PropertyId;
            expense.UnitId = dto.UnitId;
            expense.Category = dto.Category;
            expense.Amount = dto.Amount;
            expense.ExpenseDate = dto.ExpenseDate;
            expense.Vendor = dto.Vendor;
            expense.Description = dto.Description;
            expense.PaymentMethod = dto.PaymentMethod;
            expense.ReferenceNumber = dto.ReferenceNumber;
            expense.IsRecurring = dto.IsRecurring;
            expense.RecurrenceMonths = dto.RecurrenceMonths;
            expense.NextRecurrenceDate = dto.NextRecurrenceDate;
            expense.ReceiptUrl = dto.ReceiptUrl;
            expense.Notes = dto.Notes;
            expense.IsTaxDeductible = dto.IsTaxDeductible;
            expense.Tags = dto.Tags;

            // Update next recurrence date if recurring settings changed
            if (expense.IsRecurring && expense.RecurrenceMonths.HasValue && !expense.NextRecurrenceDate.HasValue)
            {
                expense.NextRecurrenceDate = expense.ExpenseDate.AddMonths(expense.RecurrenceMonths.Value);
            }

            await _expenseRepository.UpdateAsync(expense);

            _logger.LogInformation("Updated expense {ExpenseId}", id);

            // Reload with navigation properties
            var result = await _expenseRepository.GetExpenseWithDetailsAsync(id);
            return MapToDto(result!);
        }

        public async Task<bool> DeleteExpenseAsync(int id)
        {
            var expense = await _expenseRepository.GetByIdAsync(id);
            if (expense == null)
            {
                throw new InvalidOperationException($"Expense {id} not found");
            }

            if (!CanAccessLandlord(expense.LandlordId))
            {
                throw new UnauthorizedAccessException("You do not have permission to delete this expense");
            }

            await _expenseRepository.DeleteAsync(expense);

            _logger.LogInformation("Deleted expense {ExpenseId}", id);

            return true;
        }

        public async Task<ExpenseSummaryDto> GetExpenseSummaryAsync(int landlordId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!CanAccessLandlord(landlordId))
            {
                throw new UnauthorizedAccessException("You do not have permission to view these expenses");
            }

            var expenses = (await _expenseRepository.GetExpensesByLandlordIdAsync(landlordId, startDate, endDate)).ToList();

            var totalExpenses = expenses.Count;
            var totalAmount = expenses.Sum(e => e.Amount);
            var averageExpense = totalExpenses > 0 ? totalAmount / totalExpenses : 0;

            var expensesByCategory = expenses
                .GroupBy(e => e.Category.ToString())
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

            var expenseCountByCategory = expenses
                .GroupBy(e => e.Category.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            var taxDeductibleAmount = expenses.Where(e => e.IsTaxDeductible).Sum(e => e.Amount);
            var recurringExpensesTotal = expenses.Where(e => e.IsRecurring).Sum(e => e.Amount);

            return new ExpenseSummaryDto
            {
                TotalExpenses = totalExpenses,
                TotalAmount = totalAmount,
                AverageExpense = averageExpense,
                ExpensesByCategory = expensesByCategory,
                ExpenseCountByCategory = expenseCountByCategory,
                TaxDeductibleAmount = taxDeductibleAmount,
                RecurringExpensesTotal = recurringExpensesTotal
            };
        }

        public async Task<decimal> GetTotalExpensesAsync(int landlordId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!CanAccessLandlord(landlordId))
            {
                throw new UnauthorizedAccessException("You do not have permission to view these expenses");
            }

            return await _expenseRepository.GetTotalExpensesAsync(landlordId, startDate, endDate);
        }

        public async Task<decimal> GetTotalExpensesByPropertyAsync(int propertyId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!await CanAccessPropertyAsync(propertyId))
            {
                throw new UnauthorizedAccessException("You do not have permission to view expenses for this property");
            }

            return await _expenseRepository.GetTotalExpensesByPropertyAsync(propertyId, startDate, endDate);
        }

        public async Task ProcessRecurringExpensesAsync()
        {
            var today = DateTime.Today;
            var dueExpenses = await _expenseRepository.GetExpensesDueForRecurrenceAsync(today);

            foreach (var expense in dueExpenses)
            {
                try
                {
                    // Create new expense for this recurrence
                    var newExpense = new Expense
                    {
                        PropertyId = expense.PropertyId,
                        LandlordId = expense.LandlordId,
                        UnitId = expense.UnitId,
                        Category = expense.Category,
                        Amount = expense.Amount,
                        ExpenseDate = today,
                        Vendor = expense.Vendor,
                        Description = $"{expense.Description} (Recurring)",
                        PaymentMethod = expense.PaymentMethod,
                        ReferenceNumber = $"AUTO-{DateTime.Now:yyyyMMdd}-{expense.Id}",
                        IsRecurring = false, // New expense is not recurring itself
                        ReceiptUrl = expense.ReceiptUrl,
                        Notes = $"Auto-generated from recurring expense #{expense.Id}",
                        IsTaxDeductible = expense.IsTaxDeductible,
                        Tags = expense.Tags
                    };

                    await _expenseRepository.AddAsync(newExpense);

                    // Update next recurrence date
                    if (expense.RecurrenceMonths.HasValue)
                    {
                        expense.NextRecurrenceDate = expense.NextRecurrenceDate!.Value.AddMonths(expense.RecurrenceMonths.Value);
                        await _expenseRepository.UpdateAsync(expense);
                    }

                    _logger.LogInformation("Created recurring expense {NewExpenseId} from template {TemplateId}",
                        newExpense.Id, expense.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing recurring expense {ExpenseId}", expense.Id);
                }
            }
        }

        private ExpenseDto MapToDto(Expense expense)
        {
            return new ExpenseDto
            {
                Id = expense.Id,
                PropertyId = expense.PropertyId,
                PropertyName = expense.Property?.Name ?? string.Empty,
                LandlordId = expense.LandlordId,
                UnitId = expense.UnitId,
                UnitNumber = expense.Unit?.UnitNumber,
                Category = expense.Category,
                CategoryDisplay = expense.Category.ToString(),
                Amount = expense.Amount,
                ExpenseDate = expense.ExpenseDate,
                Vendor = expense.Vendor,
                Description = expense.Description,
                PaymentMethod = expense.PaymentMethod,
                PaymentMethodDisplay = expense.PaymentMethod?.ToString(),
                ReferenceNumber = expense.ReferenceNumber,
                IsRecurring = expense.IsRecurring,
                RecurrenceMonths = expense.RecurrenceMonths,
                NextRecurrenceDate = expense.NextRecurrenceDate,
                ReceiptUrl = expense.ReceiptUrl,
                Notes = expense.Notes,
                IsTaxDeductible = expense.IsTaxDeductible,
                Tags = expense.Tags,
                CreatedAt = expense.CreatedAt,
                UpdatedAt = expense.UpdatedAt
            };
        }

        private bool CanAccessExpense(Expense expense)
        {
            if (_currentUserService.IsPlatformAdmin)
            {
                return true;
            }

            if (_currentUserService.IsLandlord && _currentUserService.UserIdInt.HasValue)
            {
                return expense.LandlordId == _currentUserService.UserIdInt.Value;
            }

            if (_currentUserService.IsManager || _currentUserService.IsCaretaker || _currentUserService.IsAccountant)
            {
                var assignedPropertyIds = _currentUserService.GetAssignedPropertyIdsAsync().GetAwaiter().GetResult();
                return assignedPropertyIds.Contains(expense.PropertyId);
            }

            return false;
        }

        private bool CanAccessLandlord(int landlordId)
        {
            if (_currentUserService.IsPlatformAdmin)
            {
                return true;
            }

            if (_currentUserService.IsLandlord && _currentUserService.UserIdInt.HasValue)
            {
                return _currentUserService.UserIdInt.Value == landlordId;
            }

            return false;
        }

        private async Task<bool> CanAccessPropertyAsync(int propertyId)
        {
            if (_currentUserService.IsPlatformAdmin)
            {
                return true;
            }

            var property = await _propertyRepository.GetByIdAsync(propertyId);
            if (property == null)
            {
                return false;
            }

            if (property.LandlordId.HasValue && CanAccessLandlord(property.LandlordId.Value))
            {
                return true;
            }

            if (_currentUserService.IsManager || _currentUserService.IsCaretaker || _currentUserService.IsAccountant)
            {
                var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
                return assignedPropertyIds.Contains(propertyId);
            }

            return false;
        }
    }
}

