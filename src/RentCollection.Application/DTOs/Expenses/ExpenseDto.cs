using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.Expenses
{
    public class ExpenseDto
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public int LandlordId { get; set; }
        public int? UnitId { get; set; }
        public string? UnitNumber { get; set; }
        public ExpenseCategory Category { get; set; }
        public string CategoryDisplay { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string Vendor { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PaymentMethod? PaymentMethod { get; set; }
        public string? PaymentMethodDisplay { get; set; }
        public string? ReferenceNumber { get; set; }
        public bool IsRecurring { get; set; }
        public int? RecurrenceMonths { get; set; }
        public DateTime? NextRecurrenceDate { get; set; }
        public string? ReceiptUrl { get; set; }
        public string? Notes { get; set; }
        public bool IsTaxDeductible { get; set; }
        public string? Tags { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateExpenseDto
    {
        public int PropertyId { get; set; }
        public int? UnitId { get; set; }
        public ExpenseCategory Category { get; set; }
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string Vendor { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PaymentMethod? PaymentMethod { get; set; }
        public string? ReferenceNumber { get; set; }
        public bool IsRecurring { get; set; } = false;
        public int? RecurrenceMonths { get; set; }
        public string? ReceiptUrl { get; set; }
        public string? Notes { get; set; }
        public bool IsTaxDeductible { get; set; } = true;
        public string? Tags { get; set; }
    }

    public class UpdateExpenseDto
    {
        public int PropertyId { get; set; }
        public int? UnitId { get; set; }
        public ExpenseCategory Category { get; set; }
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string Vendor { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PaymentMethod? PaymentMethod { get; set; }
        public string? ReferenceNumber { get; set; }
        public bool IsRecurring { get; set; }
        public int? RecurrenceMonths { get; set; }
        public DateTime? NextRecurrenceDate { get; set; }
        public string? ReceiptUrl { get; set; }
        public string? Notes { get; set; }
        public bool IsTaxDeductible { get; set; }
        public string? Tags { get; set; }
    }

    public class ExpenseSummaryDto
    {
        public int TotalExpenses { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageExpense { get; set; }
        public Dictionary<string, decimal> ExpensesByCategory { get; set; } = new();
        public Dictionary<string, int> ExpenseCountByCategory { get; set; } = new();
        public decimal TaxDeductibleAmount { get; set; }
        public decimal RecurringExpensesTotal { get; set; }
    }
}
