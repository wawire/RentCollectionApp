using RentCollection.Domain.Common;

namespace RentCollection.Domain.Entities;

public class PaymentAllocation : BaseEntity
{
    public int PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;

    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;

    public decimal Amount { get; set; }
}
