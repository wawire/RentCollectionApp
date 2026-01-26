using RentCollection.Domain.Common;
using RentCollection.Domain.Enums;

namespace RentCollection.Domain.Entities;

public class InvoiceLineItem : BaseEntity
{
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;

    public InvoiceLineItemType LineItemType { get; set; } = InvoiceLineItemType.Rent;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
    public string? UnitOfMeasure { get; set; }

    public int? UtilityTypeId { get; set; }
    public UtilityType? UtilityType { get; set; }
    public int? UtilityConfigId { get; set; }
    public UtilityConfig? UtilityConfig { get; set; }
}
