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

    public async Task<byte[]> GeneratePaymentHistoryAsync(int tenantId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var tenant = await _context.Tenants
                .Include(t => t.Unit)
                .ThenInclude(u => u.Property)
                .Include(t => t.Payments)
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
            {
                throw new InvalidOperationException($"Tenant with ID {tenantId} not found");
            }

            // Filter payments by date range
            var payments = tenant.Payments.AsEnumerable();
            if (startDate.HasValue)
            {
                payments = payments.Where(p => p.PaymentDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                payments = payments.Where(p => p.PaymentDate <= endDate.Value);
            }

            var paymentsList = payments.OrderByDescending(p => p.PaymentDate).ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(header =>
                    {
                        header.Item().Background(Colors.Blue.Lighten3).Padding(15).Column(column =>
                        {
                            column.Item().Text("PAYMENT HISTORY REPORT").FontSize(20).FontColor(Colors.Blue.Darken2).Bold();
                            column.Item().Text($"Generated: {DateTime.Now:dd MMM yyyy HH:mm}").FontSize(10).FontColor(Colors.Grey.Darken1);
                        });

                        header.Item().PaddingTop(15).Column(column =>
                        {
                            column.Item().Text("TENANT INFORMATION").FontSize(12).Bold();
                            column.Item().Text($"Name: {tenant.FirstName} {tenant.LastName}");
                            column.Item().Text($"Email: {tenant.Email}");
                            column.Item().Text($"Phone: {tenant.PhoneNumber}");
                            column.Item().Text($"Property: {tenant.Unit.Property.Name} - Unit {tenant.Unit.UnitNumber}");

                            if (startDate.HasValue || endDate.HasValue)
                            {
                                var period = startDate.HasValue && endDate.HasValue
                                    ? $"{startDate:dd MMM yyyy} - {endDate:dd MMM yyyy}"
                                    : startDate.HasValue
                                        ? $"From {startDate:dd MMM yyyy}"
                                        : $"Until {endDate:dd MMM yyyy}";
                                column.Item().Text($"Period: {period}").Bold();
                            }
                        });
                    });

                    page.Content().PaddingVertical(10).Column(content =>
                    {
                        // Summary Statistics
                        var totalAmount = paymentsList.Sum(p => p.Amount);
                        var totalLateFees = paymentsList.Sum(p => p.LateFeeAmount);
                        var completedPayments = paymentsList.Where(p => p.Status == Domain.Enums.PaymentStatus.Completed);
                        var totalPaid = completedPayments.Sum(p => p.Amount + p.LateFeeAmount);
                        var pendingPayments = paymentsList.Where(p => p.Status == Domain.Enums.PaymentStatus.Pending);
                        var totalPending = pendingPayments.Sum(p => p.Amount + p.LateFeeAmount);

                        content.Item().Background(Colors.Grey.Lighten3).Padding(10).Column(summary =>
                        {
                            summary.Item().Text("PAYMENT SUMMARY").FontSize(12).Bold();
                            summary.Item().Row(row =>
                            {
                                row.RelativeItem().Text($"Total Payments: {paymentsList.Count}");
                                row.RelativeItem().Text($"Completed: {completedPayments.Count()}");
                                row.RelativeItem().Text($"Pending: {pendingPayments.Count()}");
                            });
                            summary.Item().Row(row =>
                            {
                                row.RelativeItem().Text($"Total Amount: KES {totalAmount:N2}");
                                row.RelativeItem().Text($"Total Paid: KES {totalPaid:N2}");
                                row.RelativeItem().Text($"Total Pending: KES {totalPending:N2}");
                            });
                            if (totalLateFees > 0)
                            {
                                summary.Item().Text($"Total Late Fees: KES {totalLateFees:N2}").FontColor(Colors.Red.Darken1);
                            }
                        });

                        // Payment History Table
                        content.Item().PaddingTop(15).Text("PAYMENT DETAILS").FontSize(12).Bold();
                        content.Item().PaddingTop(5).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(70);  // Date
                                columns.ConstantColumn(70);  // Due Date
                                columns.RelativeColumn();     // Period
                                columns.ConstantColumn(70);  // Amount
                                columns.ConstantColumn(60);  // Late Fee
                                columns.ConstantColumn(70);  // Total
                                columns.ConstantColumn(70);  // Status
                                columns.ConstantColumn(70);  // Method
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Date").FontColor(Colors.White).FontSize(9).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Due Date").FontColor(Colors.White).FontSize(9).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Period").FontColor(Colors.White).FontSize(9).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Amount").FontColor(Colors.White).FontSize(9).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Late Fee").FontColor(Colors.White).FontSize(9).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Total").FontColor(Colors.White).FontSize(9).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Status").FontColor(Colors.White).FontSize(9).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Method").FontColor(Colors.White).FontSize(9).Bold();
                            });

                            // Rows
                            foreach (var payment in paymentsList)
                            {
                                var isAlternate = paymentsList.IndexOf(payment) % 2 == 0;
                                var bgColor = isAlternate ? Colors.Grey.Lighten4 : Colors.White;

                                table.Cell().Background(bgColor).Padding(5).Text($"{payment.PaymentDate:dd/MM/yy}").FontSize(8);
                                table.Cell().Background(bgColor).Padding(5).Text($"{payment.DueDate:dd/MM/yy}").FontSize(8);
                                table.Cell().Background(bgColor).Padding(5).Text($"{payment.PeriodStart:MMM yy}").FontSize(8);
                                table.Cell().Background(bgColor).Padding(5).AlignRight().Text($"{payment.Amount:N2}").FontSize(8);
                                table.Cell().Background(bgColor).Padding(5).AlignRight().Text($"{payment.LateFeeAmount:N2}").FontSize(8).FontColor(payment.LateFeeAmount > 0 ? Colors.Red.Darken1 : Colors.Black);
                                table.Cell().Background(bgColor).Padding(5).AlignRight().Text($"{(payment.Amount + payment.LateFeeAmount):N2}").FontSize(8).Bold();
                                table.Cell().Background(bgColor).Padding(5).Text(payment.Status.ToString()).FontSize(8).FontColor(payment.Status == Domain.Enums.PaymentStatus.Completed ? Colors.Green.Darken1 : payment.Status == Domain.Enums.PaymentStatus.Failed ? Colors.Red.Darken1 : Colors.Orange.Darken1);
                                table.Cell().Background(bgColor).Padding(5).Text(payment.PaymentMethod.ToString()).FontSize(8);
                            }
                        });

                        // Notes if any payments have notes
                        var paymentsWithNotes = paymentsList.Where(p => !string.IsNullOrEmpty(p.Notes)).ToList();
                        if (paymentsWithNotes.Any())
                        {
                            content.Item().PaddingTop(15).Text("PAYMENT NOTES").FontSize(11).Bold();
                            foreach (var payment in paymentsWithNotes)
                            {
                                content.Item().PaddingTop(5).Text($"{payment.PaymentDate:dd MMM yyyy}: {payment.Notes}").FontSize(9).Italic();
                            }
                        }
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
                });
            });

            _logger.LogInformation("Generated payment history PDF for tenant {TenantId} with {Count} payments", tenantId, paymentsList.Count);
            return document.GeneratePdf();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating payment history PDF for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<byte[]> GeneratePropertyPaymentHistoryAsync(int propertyId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var property = await _context.Properties
                .Include(p => p.Units)
                .ThenInclude(u => u.Tenant)
                .ThenInclude(t => t!.Payments)
                .FirstOrDefaultAsync(p => p.Id == propertyId);

            if (property == null)
            {
                throw new InvalidOperationException($"Property with ID {propertyId} not found");
            }

            // Collect all payments from all tenants
            var allPayments = property.Units
                .Where(u => u.Tenant != null)
                .SelectMany(u => u.Tenant!.Payments.Select(p => new
                {
                    Payment = p,
                    TenantName = $"{u.Tenant.FirstName} {u.Tenant.LastName}",
                    UnitNumber = u.UnitNumber
                }))
                .AsEnumerable();

            // Filter by date range
            if (startDate.HasValue)
            {
                allPayments = allPayments.Where(p => p.Payment.PaymentDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                allPayments = allPayments.Where(p => p.Payment.PaymentDate <= endDate.Value);
            }

            var paymentsList = allPayments.OrderByDescending(p => p.Payment.PaymentDate).ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Column(header =>
                    {
                        header.Item().Background(Colors.Blue.Lighten3).Padding(15).Column(column =>
                        {
                            column.Item().Text("PROPERTY PAYMENT HISTORY").FontSize(18).FontColor(Colors.Blue.Darken2).Bold();
                            column.Item().Text($"Generated: {DateTime.Now:dd MMM yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Darken1);
                        });

                        header.Item().PaddingTop(10).Column(column =>
                        {
                            column.Item().Text($"Property: {property.Name}").FontSize(11).Bold();
                            column.Item().Text($"Address: {property.Address}");
                            column.Item().Text($"Total Units: {property.Units.Count} | Occupied: {property.Units.Count(u => u.Tenant != null && u.Tenant.IsActive)}");

                            if (startDate.HasValue || endDate.HasValue)
                            {
                                var period = startDate.HasValue && endDate.HasValue
                                    ? $"{startDate:dd MMM yyyy} - {endDate:dd MMM yyyy}"
                                    : startDate.HasValue
                                        ? $"From {startDate:dd MMM yyyy}"
                                        : $"Until {endDate:dd MMM yyyy}";
                                column.Item().Text($"Period: {period}").Bold();
                            }
                        });
                    });

                    page.Content().PaddingVertical(10).Column(content =>
                    {
                        // Summary Statistics
                        var totalAmount = paymentsList.Sum(p => p.Payment.Amount);
                        var totalLateFees = paymentsList.Sum(p => p.Payment.LateFeeAmount);
                        var completedPayments = paymentsList.Where(p => p.Payment.Status == Domain.Enums.PaymentStatus.Completed);
                        var totalPaid = completedPayments.Sum(p => p.Payment.Amount + p.Payment.LateFeeAmount);
                        var pendingPayments = paymentsList.Where(p => p.Payment.Status == Domain.Enums.PaymentStatus.Pending);
                        var totalPending = pendingPayments.Sum(p => p.Payment.Amount + p.Payment.LateFeeAmount);

                        content.Item().Background(Colors.Grey.Lighten3).Padding(10).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text($"Total Payments: {paymentsList.Count}");
                                col.Item().Text($"Total Amount: KES {totalAmount:N2}").Bold();
                            });
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text($"Completed: {completedPayments.Count()}");
                                col.Item().Text($"Total Paid: KES {totalPaid:N2}").FontColor(Colors.Green.Darken1).Bold();
                            });
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text($"Pending: {pendingPayments.Count()}");
                                col.Item().Text($"Total Pending: KES {totalPending:N2}").FontColor(Colors.Orange.Darken1).Bold();
                            });
                            if (totalLateFees > 0)
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Late Fees:");
                                    col.Item().Text($"KES {totalLateFees:N2}").FontColor(Colors.Red.Darken1).Bold();
                                });
                            }
                        });

                        // Payment History Table
                        content.Item().PaddingTop(15).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(60);   // Date
                                columns.ConstantColumn(80);   // Tenant
                                columns.ConstantColumn(40);   // Unit
                                columns.ConstantColumn(80);   // Period
                                columns.ConstantColumn(60);   // Amount
                                columns.ConstantColumn(50);   // Late Fee
                                columns.ConstantColumn(60);   // Total
                                columns.ConstantColumn(70);   // Status
                                columns.ConstantColumn(60);   // Method
                                columns.RelativeColumn();     // Reference
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Date").FontColor(Colors.White).FontSize(8).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Tenant").FontColor(Colors.White).FontSize(8).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Unit").FontColor(Colors.White).FontSize(8).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Period").FontColor(Colors.White).FontSize(8).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Amount").FontColor(Colors.White).FontSize(8).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Late Fee").FontColor(Colors.White).FontSize(8).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Total").FontColor(Colors.White).FontSize(8).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Status").FontColor(Colors.White).FontSize(8).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Method").FontColor(Colors.White).FontSize(8).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Reference").FontColor(Colors.White).FontSize(8).Bold();
                            });

                            // Rows
                            foreach (var item in paymentsList)
                            {
                                var isAlternate = paymentsList.IndexOf(item) % 2 == 0;
                                var bgColor = isAlternate ? Colors.Grey.Lighten4 : Colors.White;
                                var payment = item.Payment;

                                table.Cell().Background(bgColor).Padding(4).Text($"{payment.PaymentDate:dd/MM/yy}").FontSize(8);
                                table.Cell().Background(bgColor).Padding(4).Text(item.TenantName).FontSize(7);
                                table.Cell().Background(bgColor).Padding(4).Text(item.UnitNumber).FontSize(8);
                                table.Cell().Background(bgColor).Padding(4).Text($"{payment.PeriodStart:MMM yy}").FontSize(8);
                                table.Cell().Background(bgColor).Padding(4).AlignRight().Text($"{payment.Amount:N2}").FontSize(8);
                                table.Cell().Background(bgColor).Padding(4).AlignRight().Text($"{payment.LateFeeAmount:N2}").FontSize(8).FontColor(payment.LateFeeAmount > 0 ? Colors.Red.Darken1 : Colors.Black);
                                table.Cell().Background(bgColor).Padding(4).AlignRight().Text($"{(payment.Amount + payment.LateFeeAmount):N2}").FontSize(8).Bold();
                                table.Cell().Background(bgColor).Padding(4).Text(payment.Status.ToString()).FontSize(7).FontColor(payment.Status == Domain.Enums.PaymentStatus.Completed ? Colors.Green.Darken1 : payment.Status == Domain.Enums.PaymentStatus.Failed ? Colors.Red.Darken1 : Colors.Orange.Darken1);
                                table.Cell().Background(bgColor).Padding(4).Text(payment.PaymentMethod.ToString()).FontSize(7);
                                table.Cell().Background(bgColor).Padding(4).Text(payment.TransactionReference ?? "-").FontSize(7);
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
                });
            });

            _logger.LogInformation("Generated property payment history PDF for property {PropertyId} with {Count} payments", propertyId, paymentsList.Count);
            return document.GeneratePdf();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating property payment history PDF for property {PropertyId}", propertyId);
            throw;
        }
    }
}
