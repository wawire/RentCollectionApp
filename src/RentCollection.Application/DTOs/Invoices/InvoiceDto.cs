using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.Invoices;

public class InvoiceDto
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public int UnitId { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    public int PropertyId { get; set; }
    public string PropertyName { get; set; } = string.Empty;
    public int LandlordId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal Balance { get; set; }
    public InvoiceStatus Status { get; set; }
    public List<InvoiceLineItemDto> LineItems { get; set; } = new();
}
