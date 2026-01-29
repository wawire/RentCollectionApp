using FluentAssertions;
using RentCollection.Application.Helpers;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using Xunit;

namespace RentCollection.UnitTests.Invoices;

public class InvoiceStatusCalculatorTests
{
    [Fact]
    public void CalculateStatus_UnpaidBeforeDue_Is_Issued()
    {
        var invoice = new Invoice
        {
            Amount = 1000,
            OpeningBalance = 0,
            DueDate = DateTime.UtcNow.Date.AddDays(2),
            Status = InvoiceStatus.Issued
        };

        var status = InvoiceStatusCalculator.CalculateStatus(invoice, 0, DateTime.UtcNow);

        status.Should().Be(InvoiceStatus.Issued);
    }

    [Fact]
    public void CalculateStatus_UnpaidAfterDue_Is_Overdue()
    {
        var invoice = new Invoice
        {
            Amount = 1000,
            OpeningBalance = 0,
            DueDate = DateTime.UtcNow.Date.AddDays(-1),
            Status = InvoiceStatus.Issued
        };

        var status = InvoiceStatusCalculator.CalculateStatus(invoice, 0, DateTime.UtcNow);

        status.Should().Be(InvoiceStatus.Overdue);
    }

    [Fact]
    public void CalculateStatus_PartialBeforeDue_Is_PartiallyPaid()
    {
        var invoice = new Invoice
        {
            Amount = 1000,
            OpeningBalance = 0,
            DueDate = DateTime.UtcNow.Date.AddDays(3),
            Status = InvoiceStatus.Issued
        };

        var status = InvoiceStatusCalculator.CalculateStatus(invoice, 200, DateTime.UtcNow);

        status.Should().Be(InvoiceStatus.PartiallyPaid);
    }

    [Fact]
    public void CalculateStatus_PartialAfterDue_Is_Overdue()
    {
        var invoice = new Invoice
        {
            Amount = 1000,
            OpeningBalance = 0,
            DueDate = DateTime.UtcNow.Date.AddDays(-2),
            Status = InvoiceStatus.Issued
        };

        var status = InvoiceStatusCalculator.CalculateStatus(invoice, 200, DateTime.UtcNow);

        status.Should().Be(InvoiceStatus.Overdue);
    }

    [Fact]
    public void CalculateStatus_FullyPaid_Is_Paid()
    {
        var invoice = new Invoice
        {
            Amount = 1000,
            OpeningBalance = 100,
            DueDate = DateTime.UtcNow.Date.AddDays(-2),
            Status = InvoiceStatus.Issued
        };

        var status = InvoiceStatusCalculator.CalculateStatus(invoice, 1100, DateTime.UtcNow);

        status.Should().Be(InvoiceStatus.Paid);
    }
}
