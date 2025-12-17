using RentCollection.Domain.Common;
using RentCollection.Domain.Enums;

namespace RentCollection.Domain.Entities
{
    /// <summary>
    /// Represents a property-related expense for tracking profitability
    /// </summary>
    public class Expense : BaseEntity
    {
        /// <summary>
        /// Property this expense is associated with
        /// </summary>
        public int PropertyId { get; set; }
        public Property Property { get; set; } = null!;

        /// <summary>
        /// Landlord who owns the property
        /// </summary>
        public int LandlordId { get; set; }
        public User Landlord { get; set; } = null!;

        /// <summary>
        /// Optional: Specific unit if expense is unit-specific
        /// </summary>
        public int? UnitId { get; set; }
        public Unit? Unit { get; set; }

        /// <summary>
        /// Expense category (Maintenance, Utilities, etc.)
        /// </summary>
        public ExpenseCategory Category { get; set; }

        /// <summary>
        /// Amount spent
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Date when the expense was incurred
        /// </summary>
        public DateTime ExpenseDate { get; set; }

        /// <summary>
        /// Vendor/payee name (e.g., "ABC Plumbing", "City Water Department")
        /// </summary>
        public string Vendor { get; set; } = string.Empty;

        /// <summary>
        /// Description of the expense
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Payment method used
        /// </summary>
        public PaymentMethod? PaymentMethod { get; set; }

        /// <summary>
        /// Reference number (invoice number, receipt number, etc.)
        /// </summary>
        public string? ReferenceNumber { get; set; }

        /// <summary>
        /// Whether this is a recurring expense
        /// </summary>
        public bool IsRecurring { get; set; } = false;

        /// <summary>
        /// Recurrence interval in months (1 = monthly, 3 = quarterly, 12 = annually)
        /// </summary>
        public int? RecurrenceMonths { get; set; }

        /// <summary>
        /// Next scheduled date for recurring expense
        /// </summary>
        public DateTime? NextRecurrenceDate { get; set; }

        /// <summary>
        /// Receipt/invoice file path or URL
        /// </summary>
        public string? ReceiptUrl { get; set; }

        /// <summary>
        /// Additional notes
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Whether this expense is tax deductible
        /// </summary>
        public bool IsTaxDeductible { get; set; } = true;

        /// <summary>
        /// Tags for filtering and categorization
        /// </summary>
        public string? Tags { get; set; }  // Comma-separated tags
    }
}
