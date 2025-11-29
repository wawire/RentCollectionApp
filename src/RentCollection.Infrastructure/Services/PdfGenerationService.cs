using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Services;

public class PdfGenerationService : IPdfService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PdfGenerationService> _logger;

    public PdfGenerationService(ApplicationDbContext context, ILogger<PdfGenerationService> logger)
    {
        _context = context;
        _logger = logger;

        // Set QuestPDF license (Community license for non-commercial use)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GeneratePaymentReceiptAsync(int paymentId)
    {
        try
        {
            var payment = await _context.Payments
                .Include(p => p.Tenant)
                .ThenInclude(t => t.Unit)
                .ThenInclude(u => u.Property)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                throw new InvalidOperationException($"Payment with ID {paymentId} not found");
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .Height(100)
                        .Background(Colors.Blue.Lighten3)
                        .Padding(20)
                        .Column(column =>
                        {
                            column.Item().Text("PAYMENT RECEIPT")
                                .FontSize(24)
                                .FontColor(Colors.Blue.Darken2)
                                .Bold();

                            column.Item().Text("Property Management System")
                                .FontSize(12)
                                .FontColor(Colors.Grey.Darken1);
                        });

                    page.Content()
                        .PaddingVertical(20)
                        .Column(column =>
                        {
                            // Receipt Details Section
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Column(leftColumn =>
                                {
                                    leftColumn.Item().Text($"Receipt No: {payment.TransactionReference ?? payment.Id.ToString()}")
                                        .Bold();
                                    leftColumn.Item().Text($"Date: {payment.PaymentDate:dd MMM yyyy}");
                                });

                                row.RelativeItem().Column(rightColumn =>
                                {
                                    rightColumn.Item().AlignRight().Text($"Payment Method: {payment.PaymentMethod}");
                                    rightColumn.Item().AlignRight().Text($"Status: {payment.Status}")
                                        .FontColor(payment.Status.ToString() == "Completed" ? Colors.Green.Darken1 : Colors.Orange.Darken1);
                                });
                            });

                            column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            // Tenant Information
                            column.Item().PaddingTop(15).Text("TENANT INFORMATION").Bold().FontSize(14);
                            column.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(150);
                                    columns.RelativeColumn();
                                });

                                table.Cell().Text("Name:").Bold();
                                table.Cell().Text($"{payment.Tenant.FirstName} {payment.Tenant.LastName}");

                                table.Cell().Text("Phone:").Bold();
                                table.Cell().Text(payment.Tenant.PhoneNumber);

                                table.Cell().Text("Email:").Bold();
                                table.Cell().Text(payment.Tenant.Email);

                                table.Cell().Text("Property:").Bold();
                                table.Cell().Text(payment.Tenant.Unit.Property.Name);

                                table.Cell().Text("Unit:").Bold();
                                table.Cell().Text(payment.Tenant.Unit.UnitNumber);
                            });

                            column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            // Payment Details
                            column.Item().PaddingTop(15).Text("PAYMENT DETAILS").Bold().FontSize(14);
                            column.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                });

                                // Header
                                table.Cell().Element(CellStyle).Text("Description").Bold();
                                table.Cell().Element(CellStyle).AlignRight().Text("Amount").Bold();

                                // Amount
                                table.Cell().Element(CellStyle).Text($"Rent Payment - {payment.PaymentDate:MMMM yyyy}");
                                table.Cell().Element(CellStyle).AlignRight().Text($"KES {payment.Amount:N2}");

                                // Total
                                table.Cell().Element(CellStyle).Background(Colors.Grey.Lighten3).Text("TOTAL").Bold();
                                table.Cell().Element(CellStyle).Background(Colors.Grey.Lighten3).AlignRight().Text($"KES {payment.Amount:N2}").Bold();

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                                }
                            });

                            // Notes
                            if (!string.IsNullOrWhiteSpace(payment.Notes))
                            {
                                column.Item().PaddingTop(20).Text("NOTES").Bold();
                                column.Item().PaddingTop(5).Text(payment.Notes);
                            }

                            // Thank you message
                            column.Item().PaddingTop(30).AlignCenter().Text("Thank you for your payment!")
                                .FontSize(12)
                                .Italic()
                                .FontColor(Colors.Grey.Darken1);
                        });

                    page.Footer()
                        .AlignCenter()
                        .DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Darken1))
                        .Text(text =>
                        {
                            text.Span("Generated on ");
                            text.Span(DateTime.UtcNow.ToString("dd MMM yyyy HH:mm")).Bold();
                            text.Span(" | This is a computer-generated receipt and does not require a signature.");
                        });
                });
            });

            _logger.LogInformation("Generated payment receipt PDF for payment {PaymentId}", paymentId);
            return document.GeneratePdf();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating payment receipt PDF for payment {PaymentId}", paymentId);
            throw;
        }
    }

    public async Task<byte[]> GenerateMonthlyReportAsync(int year, int month)
    {
        try
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var payments = await _context.Payments
                .Include(p => p.Tenant)
                .ThenInclude(t => t.Unit)
                .ThenInclude(u => u.Property)
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .OrderBy(p => p.PaymentDate)
                .ToListAsync();

            var totalRevenue = payments.Sum(p => p.Amount);
            var totalPayments = payments.Count;
            var totalProperties = await _context.Properties.CountAsync();
            var totalUnits = await _context.Units.CountAsync();
            var occupiedUnits = await _context.Units.CountAsync(u => u.IsOccupied);
            var occupancyRate = totalUnits > 0 ? (occupiedUnits * 100.0 / totalUnits) : 0;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .Height(120)
                        .Background(Colors.Blue.Lighten3)
                        .Padding(20)
                        .Column(column =>
                        {
                            column.Item().Text("MONTHLY FINANCIAL REPORT")
                                .FontSize(24)
                                .FontColor(Colors.Blue.Darken2)
                                .Bold();

                            column.Item().Text($"{startDate:MMMM yyyy}")
                                .FontSize(16)
                                .FontColor(Colors.Grey.Darken1);

                            column.Item().Text("Property Management System")
                                .FontSize(12)
                                .FontColor(Colors.Grey.Darken1);
                        });

                    page.Content()
                        .PaddingVertical(20)
                        .Column(column =>
                        {
                            // Summary Statistics
                            column.Item().Text("SUMMARY STATISTICS").Bold().FontSize(14);
                            column.Item().PaddingTop(10).Row(row =>
                            {
                                row.RelativeItem().Background(Colors.Blue.Lighten4).Padding(15).Column(statColumn =>
                                {
                                    statColumn.Item().AlignCenter().Text("Total Revenue").Bold().FontSize(12);
                                    statColumn.Item().AlignCenter().Text($"KES {totalRevenue:N2}").FontSize(18).FontColor(Colors.Green.Darken1);
                                });

                                row.Spacing(10);

                                row.RelativeItem().Background(Colors.Green.Lighten4).Padding(15).Column(statColumn =>
                                {
                                    statColumn.Item().AlignCenter().Text("Total Payments").Bold().FontSize(12);
                                    statColumn.Item().AlignCenter().Text(totalPayments.ToString()).FontSize(18).FontColor(Colors.Blue.Darken1);
                                });

                                row.Spacing(10);

                                row.RelativeItem().Background(Colors.Orange.Lighten4).Padding(15).Column(statColumn =>
                                {
                                    statColumn.Item().AlignCenter().Text("Occupancy Rate").Bold().FontSize(12);
                                    statColumn.Item().AlignCenter().Text($"{occupancyRate:F1}%").FontSize(18).FontColor(Colors.Orange.Darken1);
                                });
                            });

                            column.Item().PaddingVertical(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            // Property Overview
                            column.Item().PaddingTop(10).Text("PROPERTY OVERVIEW").Bold().FontSize(14);
                            column.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(200);
                                    columns.RelativeColumn();
                                });

                                table.Cell().Text("Total Properties:").Bold();
                                table.Cell().Text(totalProperties.ToString());

                                table.Cell().Text("Total Units:").Bold();
                                table.Cell().Text(totalUnits.ToString());

                                table.Cell().Text("Occupied Units:").Bold();
                                table.Cell().Text(occupiedUnits.ToString());

                                table.Cell().Text("Vacant Units:").Bold();
                                table.Cell().Text((totalUnits - occupiedUnits).ToString());
                            });

                            column.Item().PaddingVertical(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            // Payment Summary
                            column.Item().PaddingTop(10).Text("PAYMENT SUMMARY").Bold().FontSize(14);
                            column.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(80);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                // Header
                                table.Cell().Element(HeaderStyle).Text("Date");
                                table.Cell().Element(HeaderStyle).Text("Tenant");
                                table.Cell().Element(HeaderStyle).Text("Property/Unit");
                                table.Cell().Element(HeaderStyle).AlignRight().Text("Amount");
                                table.Cell().Element(HeaderStyle).Text("Method");

                                // Data rows
                                foreach (var payment in payments)
                                {
                                    table.Cell().Element(CellStyle).Text(payment.PaymentDate.ToString("dd MMM"));
                                    table.Cell().Element(CellStyle).Text($"{payment.Tenant.FirstName} {payment.Tenant.LastName}");
                                    table.Cell().Element(CellStyle).Text($"{payment.Tenant.Unit.Property.Name} - {payment.Tenant.Unit.UnitNumber}");
                                    table.Cell().Element(CellStyle).AlignRight().Text($"KES {payment.Amount:N2}");
                                    table.Cell().Element(CellStyle).Text(payment.PaymentMethod.ToString());
                                }

                                // Total row - spans first 3 columns
                                table.Cell().Element(TotalStyle).Text("TOTAL").Bold();
                                table.Cell().Element(TotalStyle).Text("");
                                table.Cell().Element(TotalStyle).Text("");
                                table.Cell().Element(TotalStyle).AlignRight().Text($"KES {totalRevenue:N2}").Bold();
                                table.Cell().Element(TotalStyle);

                                static IContainer HeaderStyle(IContainer container)
                                {
                                    return container.Background(Colors.Blue.Lighten3).BorderBottom(1).BorderColor(Colors.Blue.Darken1).Padding(5);
                                }

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
                                }

                                static IContainer TotalStyle(IContainer container)
                                {
                                    return container.Background(Colors.Grey.Lighten3).BorderTop(2).BorderColor(Colors.Grey.Darken1).Padding(5);
                                }
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Darken1))
                        .Text(text =>
                        {
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                            text.Span(" | Generated on ");
                            text.Span(DateTime.UtcNow.ToString("dd MMM yyyy HH:mm"));
                        });
                });
            });

            _logger.LogInformation("Generated monthly report PDF for {Year}-{Month}", year, month);
            return document.GeneratePdf();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating monthly report PDF for {Year}-{Month}", year, month);
            throw;
        }
    }

    public async Task<byte[]> GenerateTenantListAsync()
    {
        try
        {
            var tenants = await _context.Tenants
                .Include(t => t.Unit)
                .ThenInclude(u => u.Property)
                .Where(t => t.IsActive)
                .OrderBy(t => t.Unit.Property.Name)
                .ThenBy(t => t.Unit.UnitNumber)
                .ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Height(100)
                        .Background(Colors.Blue.Lighten3)
                        .Padding(20)
                        .Column(column =>
                        {
                            column.Item().Text("TENANT LIST")
                                .FontSize(24)
                                .FontColor(Colors.Blue.Darken2)
                                .Bold();

                            column.Item().Text($"Active Tenants: {tenants.Count}")
                                .FontSize(14)
                                .FontColor(Colors.Grey.Darken1);

                            column.Item().Text("Property Management System")
                                .FontSize(12)
                                .FontColor(Colors.Grey.Darken1);
                        });

                    page.Content()
                        .PaddingVertical(20)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            // Header
                            table.Cell().Element(HeaderStyle).Text("Tenant Name");
                            table.Cell().Element(HeaderStyle).Text("Property");
                            table.Cell().Element(HeaderStyle).Text("Unit");
                            table.Cell().Element(HeaderStyle).Text("Phone");
                            table.Cell().Element(HeaderStyle).Text("Email");
                            table.Cell().Element(HeaderStyle).AlignRight().Text("Monthly Rent");
                            table.Cell().Element(HeaderStyle).Text("Move-in Date");

                            // Data rows
                            foreach (var tenant in tenants)
                            {
                                table.Cell().Element(CellStyle).Text($"{tenant.FirstName} {tenant.LastName}");
                                table.Cell().Element(CellStyle).Text(tenant.Unit.Property.Name);
                                table.Cell().Element(CellStyle).Text(tenant.Unit.UnitNumber);
                                table.Cell().Element(CellStyle).Text(tenant.PhoneNumber);
                                table.Cell().Element(CellStyle).Text(tenant.Email);
                                table.Cell().Element(CellStyle).AlignRight().Text($"KES {tenant.MonthlyRent:N2}");
                                table.Cell().Element(CellStyle).Text(tenant.LeaseStartDate.ToString("dd MMM yyyy"));
                            }

                            static IContainer HeaderStyle(IContainer container)
                            {
                                return container.Background(Colors.Blue.Lighten3).BorderBottom(2).BorderColor(Colors.Blue.Darken1).Padding(5);
                            }

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Darken1))
                        .Text(text =>
                        {
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                            text.Span(" | Generated on ");
                            text.Span(DateTime.UtcNow.ToString("dd MMM yyyy HH:mm"));
                        });
                });
            });

            _logger.LogInformation("Generated tenant list PDF with {Count} tenants", tenants.Count);
            return document.GeneratePdf();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating tenant list PDF");
            throw;
        }
    }
}
