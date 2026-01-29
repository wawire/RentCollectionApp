using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Helpers;

public static class InvoiceStatusCalculator
{
    public static decimal CalculateBalance(Invoice invoice, decimal allocatedTotal)
    {
        var total = invoice.Amount + invoice.OpeningBalance;
        return Math.Max(0, total - allocatedTotal);
    }

    public static InvoiceStatus CalculateStatus(Invoice invoice, decimal allocatedTotal, DateTime utcNow)
    {
        if (invoice.Status == InvoiceStatus.Void)
        {
            return InvoiceStatus.Void;
        }

        var balance = CalculateBalance(invoice, allocatedTotal);
        if (balance <= 0)
        {
            return InvoiceStatus.Paid;
        }

        if (invoice.DueDate.Date < utcNow.Date)
        {
            return InvoiceStatus.Overdue;
        }

        if (allocatedTotal > 0)
        {
            return InvoiceStatus.PartiallyPaid;
        }

        return invoice.Status == InvoiceStatus.Draft ? InvoiceStatus.Draft : InvoiceStatus.Issued;
    }

    public static bool Apply(Invoice invoice, decimal allocatedTotal, DateTime utcNow)
    {
        var newBalance = CalculateBalance(invoice, allocatedTotal);
        var newStatus = CalculateStatus(invoice, allocatedTotal, utcNow);

        var changed = invoice.Balance != newBalance || invoice.Status != newStatus;
        invoice.Balance = newBalance;
        invoice.Status = newStatus;

        return changed;
    }
}
