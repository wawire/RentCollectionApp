using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.Invoices;

public class InvoiceLineItemDto
{
    public int Id { get; set; }
    public InvoiceLineItemType LineItemType { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
    public string? UnitOfMeasure { get; set; }
    public int? UtilityTypeId { get; set; }
    public string? UtilityName { get; set; }
}
