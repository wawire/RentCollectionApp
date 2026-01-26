using RentCollection.Domain.Common;
using RentCollection.Domain.Enums;

namespace RentCollection.Domain.Entities;

public class Invoice : BaseEntity
{
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;

    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;

    public int LandlordId { get; set; }
    public User Landlord { get; set; } = null!;

    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime DueDate { get; set; }

    public decimal Amount { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal Balance { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Issued;
    public string? Notes { get; set; }

    public ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();
    public ICollection<PaymentAllocation> Allocations { get; set; } = new List<PaymentAllocation>();
}
