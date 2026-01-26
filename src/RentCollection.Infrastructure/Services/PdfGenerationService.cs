using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Infrastructure.Data;
using RentCollection.Domain.Enums;

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

    public async Task<byte[]> GenerateMonthlyReportAsync(int year, int month, int? landlordId = null, IReadOnlyCollection<int>? propertyIds = null, int? organizationId = null)
    {
        try
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var paymentsQuery = _context.Payments
                .Include(p => p.Tenant)
                .ThenInclude(t => t.Unit)
                .ThenInclude(u => u.Property)
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .AsQueryable();

            if (organizationId.HasValue)
            {
                paymentsQuery = paymentsQuery.Where(p => p.Tenant.Unit.Property.OrganizationId == organizationId.Value);
            }

            if (landlordId.HasValue)
            {
                paymentsQuery = paymentsQuery.Where(p => p.Tenant.Unit.Property.LandlordId == landlordId.Value);
            }
            else if (propertyIds != null && propertyIds.Count > 0)
            {
                paymentsQuery = paymentsQuery.Where(p => propertyIds.Contains(p.Tenant.Unit.PropertyId));
            }

            var payments = await paymentsQuery
                .OrderBy(p => p.PaymentDate)
                .ToListAsync();

            var totalRevenue = payments.Sum(p => p.Amount);
            var totalPayments = payments.Count;
            var propertiesQuery = _context.Properties
                .Include(p => p.Units)
                .AsQueryable();

            if (organizationId.HasValue)
            {
                propertiesQuery = propertiesQuery.Where(p => p.OrganizationId == organizationId.Value);
            }

            if (landlordId.HasValue)
            {
                propertiesQuery = propertiesQuery.Where(p => p.LandlordId == landlordId.Value);
            }
            else if (propertyIds != null && propertyIds.Count > 0)
            {
                propertiesQuery = propertiesQuery.Where(p => propertyIds.Contains(p.Id));
            }

            var properties = await propertiesQuery.ToListAsync();
            var totalProperties = properties.Count;
            var totalUnits = properties.Sum(p => p.Units.Count);
            var occupiedUnits = properties.Sum(p => p.Units.Count(u => u.IsOccupied));
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

    public async Task<byte[]> GenerateTenantListAsync(int? landlordId = null, IReadOnlyCollection<int>? propertyIds = null, int? organizationId = null)
    {
        try
        {
            var tenantsQuery = _context.Tenants
                .Include(t => t.Unit)
                .ThenInclude(u => u.Property)
                .Where(t => t.IsActive)
                .AsQueryable();

            if (organizationId.HasValue)
            {
                tenantsQuery = tenantsQuery.Where(t => t.Unit.Property.OrganizationId == organizationId.Value);
            }

            if (landlordId.HasValue)
            {
                tenantsQuery = tenantsQuery.Where(t => t.Unit.Property.LandlordId == landlordId.Value);
            }
            else if (propertyIds != null && propertyIds.Count > 0)
            {
                tenantsQuery = tenantsQuery.Where(t => propertyIds.Contains(t.Unit.PropertyId));
            }

            var tenants = await tenantsQuery
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
                .ThenInclude(u => u.Tenants)
                .ThenInclude(t => t.Payments)
                .FirstOrDefaultAsync(p => p.Id == propertyId);

            if (property == null)
            {
                throw new InvalidOperationException($"Property with ID {propertyId} not found");
            }

            // Collect all payments from all tenants
            var allPayments = property.Units
                .SelectMany(u => u.Tenants.Where(t => t.IsActive).SelectMany(t => t.Payments.Select(p => new
                {
                    Payment = p,
                    TenantName = t.FullName,
                    UnitNumber = u.UnitNumber
                })))
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
                            column.Item().Text($"Address: {property.Location}");
                            column.Item().Text($"Total Units: {property.Units.Count} | Occupied: {property.Units.Count(u => u.Tenants.Any(t => t.IsActive))}");

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

    public async Task<byte[]> GenerateMoveOutInspectionReportAsync(int inspectionId)
    {
        try
        {
            var inspection = await _context.MoveOutInspections
                .Include(i => i.Tenant)
                .Include(i => i.Unit)
                    .ThenInclude(u => u.Property)
                .Include(i => i.InspectionItems)
                .Include(i => i.Photos)
                .Include(i => i.InspectedBy)
                .FirstOrDefaultAsync(i => i.Id == inspectionId);

            if (inspection == null)
            {
                throw new InvalidOperationException($"Move-out inspection with ID {inspectionId} not found");
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Height(120)
                        .Background(Colors.Purple.Lighten3)
                        .Padding(20)
                        .Column(column =>
                        {
                            column.Item().Text("MOVE-OUT INSPECTION REPORT")
                                .FontSize(24)
                                .FontColor(Colors.Purple.Darken2)
                                .Bold();

                            column.Item().Text($"Inspection #{inspection.Id}")
                                .FontSize(14)
                                .FontColor(Colors.Grey.Darken1);

                            column.Item().Text($"Status: {inspection.Status}")
                                .FontSize(12)
                                .FontColor(Colors.Grey.Darken1);
                        });

                    page.Content()
                        .PaddingVertical(15)
                        .Column(column =>
                        {
                            // Property and Tenant Information
                            column.Item().Row(row =>
                            {
                                // Left column - Property info
                                row.RelativeItem().Column(leftCol =>
                                {
                                    leftCol.Item().Text("PROPERTY INFORMATION").Bold().FontSize(12);
                                    leftCol.Item().PaddingTop(5).Table(table =>
                                    {
                                        table.ColumnsDefinition(cols =>
                                        {
                                            cols.ConstantColumn(100);
                                            cols.RelativeColumn();
                                        });

                                        table.Cell().Text("Property:").Bold();
                                        table.Cell().Text(inspection.Unit.Property.Name);

                                        table.Cell().Text("Unit:").Bold();
                                        table.Cell().Text(inspection.Unit.UnitNumber);

                                        table.Cell().Text("Address:").Bold();
                                        table.Cell().Text(inspection.Unit.Property.Location ?? "N/A");
                                    });
                                });

                                row.Spacing(15);

                                // Right column - Tenant info
                                row.RelativeItem().Column(rightCol =>
                                {
                                    rightCol.Item().Text("TENANT INFORMATION").Bold().FontSize(12);
                                    rightCol.Item().PaddingTop(5).Table(table =>
                                    {
                                        table.ColumnsDefinition(cols =>
                                        {
                                            cols.ConstantColumn(100);
                                            cols.RelativeColumn();
                                        });

                                        table.Cell().Text("Name:").Bold();
                                        table.Cell().Text(inspection.Tenant.FullName);

                                        table.Cell().Text("Phone:").Bold();
                                        table.Cell().Text(inspection.Tenant.PhoneNumber);

                                        table.Cell().Text("Email:").Bold();
                                        table.Cell().Text(inspection.Tenant.Email);
                                    });
                                });
                            });

                            column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            // Inspection Details
                            column.Item().Text("INSPECTION DETAILS").Bold().FontSize(12);
                            column.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.ConstantColumn(150);
                                    cols.RelativeColumn();
                                    cols.ConstantColumn(150);
                                    cols.RelativeColumn();
                                });

                                table.Cell().Text("Move-Out Date:").Bold();
                                table.Cell().Text(inspection.MoveOutDate.ToString("dd MMM yyyy"));

                                table.Cell().Text("Inspection Date:").Bold();
                                table.Cell().Text(inspection.InspectionDate.ToString("dd MMM yyyy"));

                                table.Cell().Text("Inspected By:").Bold();
                                table.Cell().Text(inspection.InspectedBy?.FullName ?? "N/A");

                                table.Cell().Text("Overall Condition:").Bold();
                                table.Cell().Text(inspection.OverallCondition ?? "Not specified");
                            });

                            if (!string.IsNullOrEmpty(inspection.GeneralNotes))
                            {
                                column.Item().PaddingTop(10).Column(noteCol =>
                                {
                                    noteCol.Item().Text("General Notes:").Bold();
                                    noteCol.Item().Text(inspection.GeneralNotes).Italic();
                                });
                            }

                            column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            // Inspection Items
                            if (inspection.InspectionItems.Any())
                            {
                                column.Item().Text("INSPECTION ITEMS").Bold().FontSize(12);
                                column.Item().PaddingTop(10).Table(table =>
                                {
                                    table.ColumnsDefinition(cols =>
                                    {
                                        cols.RelativeColumn(2);
                                        cols.RelativeColumn(2);
                                        cols.RelativeColumn(2);
                                        cols.ConstantColumn(80);
                                        cols.ConstantColumn(70);
                                    });

                                    // Header
                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Purple.Lighten3).Padding(5).Text("Item").Bold();
                                        header.Cell().Background(Colors.Purple.Lighten3).Padding(5).Text("Move-In").Bold();
                                        header.Cell().Background(Colors.Purple.Lighten3).Padding(5).Text("Move-Out").Bold();
                                        header.Cell().Background(Colors.Purple.Lighten3).Padding(5).Text("Damaged").Bold();
                                        header.Cell().Background(Colors.Purple.Lighten3).Padding(5).AlignRight().Text("Cost").Bold();
                                    });

                                    // Rows
                                    foreach (var item in inspection.InspectionItems)
                                    {
                                        var bgColor = item.IsDamaged ? Colors.Red.Lighten4 : Colors.White;

                                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{item.Category}: {item.ItemName}").FontSize(9);
                                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.MoveInCondition ?? "N/A").FontSize(9);
                                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.MoveOutCondition ?? "N/A").FontSize(9);
                                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.IsDamaged ? "Yes" : "No").FontSize(9).FontColor(item.IsDamaged ? Colors.Red.Darken1 : Colors.Green.Darken1);
                                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"KES {item.EstimatedRepairCost:N2}").FontSize(9);

                                        if (item.IsDamaged && !string.IsNullOrEmpty(item.DamageDescription))
                                        {
                                            table.Cell().ColumnSpan(5).Background(Colors.Red.Lighten5).Padding(5).Text($"Damage: {item.DamageDescription}").FontSize(8).Italic();
                                        }
                                    }
                                });

                                column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                            }

                            // Financial Summary
                            column.Item().Text("FINANCIAL SUMMARY").Bold().FontSize(14).FontColor(Colors.Purple.Darken2);
                            column.Item().PaddingTop(10).Background(Colors.Grey.Lighten4).Padding(10).Column(finCol =>
                            {
                                finCol.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("Security Deposit Held:").Bold();
                                    row.ConstantItem(120).AlignRight().Text($"KES {inspection.SecurityDepositHeld:N2}").Bold().FontSize(12);
                                });

                                finCol.Item().PaddingTop(8).Text("DEDUCTIONS").Bold().FontColor(Colors.Red.Darken1);

                                if (inspection.CleaningCharges > 0)
                                {
                                    finCol.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text("Cleaning Charges:");
                                        row.ConstantItem(120).AlignRight().Text($"-KES {inspection.CleaningCharges:N2}").FontColor(Colors.Red.Darken1);
                                    });
                                }

                                if (inspection.RepairCharges > 0)
                                {
                                    finCol.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text("Repair Charges:");
                                        row.ConstantItem(120).AlignRight().Text($"-KES {inspection.RepairCharges:N2}").FontColor(Colors.Red.Darken1);
                                    });
                                }

                                if (inspection.UnpaidRent > 0)
                                {
                                    finCol.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text("Unpaid Rent:");
                                        row.ConstantItem(120).AlignRight().Text($"-KES {inspection.UnpaidRent:N2}").FontColor(Colors.Red.Darken1);
                                    });
                                }

                                if (inspection.UnpaidUtilities > 0)
                                {
                                    finCol.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text("Unpaid Utilities:");
                                        row.ConstantItem(120).AlignRight().Text($"-KES {inspection.UnpaidUtilities:N2}").FontColor(Colors.Red.Darken1);
                                    });
                                }

                                if (inspection.OtherCharges > 0)
                                {
                                    finCol.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text("Other Charges:");
                                        row.ConstantItem(120).AlignRight().Text($"-KES {inspection.OtherCharges:N2}").FontColor(Colors.Red.Darken1);
                                    });
                                }

                                finCol.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Darken1);

                                finCol.Item().PaddingTop(5).Row(row =>
                                {
                                    row.RelativeItem().Text("TOTAL DEDUCTIONS:").Bold();
                                    row.ConstantItem(120).AlignRight().Text($"-KES {inspection.TotalDeductions:N2}").Bold().FontColor(Colors.Red.Darken2);
                                });

                                finCol.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Purple.Darken1);

                                finCol.Item().PaddingTop(8).Background(Colors.Green.Lighten4).Padding(8).Row(row =>
                                {
                                    row.RelativeItem().Text("REFUND AMOUNT:").Bold().FontSize(14);
                                    row.ConstantItem(120).AlignRight().Text($"KES {inspection.RefundAmount:N2}").Bold().FontSize(14).FontColor(Colors.Green.Darken2);
                                });

                                if (inspection.TenantOwes > 0)
                                {
                                    finCol.Item().PaddingTop(5).Background(Colors.Red.Lighten4).Padding(8).Row(row =>
                                    {
                                        row.RelativeItem().Text("TENANT OWES:").Bold().FontSize(12);
                                        row.ConstantItem(120).AlignRight().Text($"KES {inspection.TenantOwes:N2}").Bold().FontSize(12).FontColor(Colors.Red.Darken2);
                                    });
                                }
                            });

                            // Refund Information
                            if (inspection.RefundProcessed)
                            {
                                column.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                                column.Item().PaddingTop(10).Background(Colors.Teal.Lighten4).Padding(10).Column(refundCol =>
                                {
                                    refundCol.Item().Text("REFUND PROCESSED").Bold().FontSize(12).FontColor(Colors.Teal.Darken2);
                                    refundCol.Item().PaddingTop(5).Table(table =>
                                    {
                                        table.ColumnsDefinition(cols =>
                                        {
                                            cols.ConstantColumn(120);
                                            cols.RelativeColumn();
                                        });

                                        table.Cell().Text("Method:").Bold();
                                        table.Cell().Text(inspection.RefundMethod ?? "N/A");

                                        if (!string.IsNullOrEmpty(inspection.RefundReference))
                                        {
                                            table.Cell().Text("Reference:").Bold();
                                            table.Cell().Text(inspection.RefundReference);
                                        }

                                        if (inspection.RefundDate.HasValue)
                                        {
                                            table.Cell().Text("Date:").Bold();
                                            table.Cell().Text(inspection.RefundDate.Value.ToString("dd MMM yyyy"));
                                        }
                                    });
                                });
                            }

                            // Settlement Notes
                            if (inspection.IsSettled && !string.IsNullOrEmpty(inspection.SettlementNotes))
                            {
                                column.Item().PaddingTop(15).Column(settleCol =>
                                {
                                    settleCol.Item().Text("SETTLEMENT NOTES").Bold();
                                    settleCol.Item().Background(Colors.Grey.Lighten4).Padding(10).Text(inspection.SettlementNotes);
                                });
                            }

                            // Signatures
                            column.Item().PaddingTop(30).Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                                    col.Item().PaddingTop(5).Text("Landlord Signature").FontSize(9);
                                    col.Item().Text($"Date: {DateTime.Now:dd MMM yyyy}").FontSize(8);
                                });

                                row.Spacing(30);

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                                    col.Item().PaddingTop(5).Text("Tenant Signature").FontSize(9);
                                    col.Item().Text($"Name: {inspection.Tenant.FullName}").FontSize(8);
                                });
                            });

                            // Footer disclaimer
                            column.Item().PaddingTop(20).AlignCenter().Text("This is a computer-generated document. Both parties acknowledge the accuracy of the information above.")
                                .FontSize(8)
                                .Italic()
                                .FontColor(Colors.Grey.Darken1);
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
                            text.Span(DateTime.UtcNow.ToString("dd MMM yyyy HH:mm")).Bold();
                        });
                });
            });

            _logger.LogInformation("Generated move-out inspection report PDF for inspection {InspectionId}", inspectionId);
            return document.GeneratePdf();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating move-out inspection report PDF for inspection {InspectionId}", inspectionId);
            throw;
        }
    }

    public async Task<byte[]> GenerateOwnerStatementAsync(int landlordId, int year, int month, int? propertyId = null)
    {
        var periodStart = new DateTime(year, month, 1);
        var periodEnd = periodStart.AddMonths(1).AddDays(-1);

        var invoiceQuery = _context.Invoices
            .Include(i => i.Property)
            .Include(i => i.Unit)
            .Include(i => i.Tenant)
            .Include(i => i.LineItems)
                .ThenInclude(li => li.UtilityType)
            .Where(i => i.LandlordId == landlordId);

        if (propertyId.HasValue)
        {
            invoiceQuery = invoiceQuery.Where(i => i.PropertyId == propertyId.Value);
        }

        var openingArrears = await invoiceQuery
            .Where(i => i.PeriodEnd < periodStart && i.Balance > 0 && i.Status != InvoiceStatus.Void)
            .SumAsync(i => i.Balance);

        var periodInvoices = await invoiceQuery
            .Where(i => i.PeriodStart >= periodStart && i.PeriodEnd <= periodEnd)
            .ToListAsync();

        var invoiceTotal = periodInvoices.Sum(i => i.Amount + i.OpeningBalance);
        var utilityTotals = periodInvoices
            .SelectMany(i => i.LineItems)
            .Where(li => li.LineItemType == InvoiceLineItemType.Utility)
            .GroupBy(li => li.UtilityType?.Name ?? "Utility")
            .Select(group => new { UtilityName = group.Key, Total = group.Sum(li => li.Amount) })
            .ToList();

        var paymentsQuery = _context.Payments
            .Include(p => p.Unit)
                .ThenInclude(u => u.Property)
            .Where(p => p.Status == PaymentStatus.Completed &&
                        p.PaymentDate >= periodStart &&
                        p.PaymentDate <= periodEnd);

        if (propertyId.HasValue)
        {
            paymentsQuery = paymentsQuery.Where(p =>
                p.Unit.PropertyId == propertyId.Value &&
                p.Unit.Property.LandlordId == landlordId);
        }
        else
        {
            paymentsQuery = paymentsQuery.Where(p => p.Unit.Property.LandlordId == landlordId);
        }

        var paymentsTotal = await paymentsQuery.SumAsync(p => p.Amount);

        var expensesQuery = _context.Expenses
            .Include(e => e.Property)
            .Where(e => e.LandlordId == landlordId &&
                        e.ExpenseDate >= periodStart &&
                        e.ExpenseDate <= periodEnd);

        if (propertyId.HasValue)
        {
            expensesQuery = expensesQuery.Where(e => e.PropertyId == propertyId.Value);
        }

        var expensesTotal = await expensesQuery.SumAsync(e => e.Amount);

        var closingArrears = await invoiceQuery
            .Where(i => i.PeriodEnd <= periodEnd && i.Balance > 0 && i.Status != InvoiceStatus.Void)
            .SumAsync(i => i.Balance);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .Height(90)
                    .Background(Colors.Blue.Lighten3)
                    .Padding(20)
                    .Column(column =>
                    {
                        column.Item().Text("OWNER STATEMENT")
                            .FontSize(22)
                            .FontColor(Colors.Blue.Darken2)
                            .Bold();

                        column.Item().Text($"{periodStart:MMMM yyyy}")
                            .FontSize(14)
                            .FontColor(Colors.Grey.Darken1);
                    });

                page.Content()
                    .PaddingVertical(15)
                    .Column(column =>
                    {
                        column.Item().Text("SUMMARY").Bold().FontSize(14);

                        column.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(200);
                                cols.RelativeColumn();
                            });

                            table.Cell().Text("Opening Arrears:").Bold();
                            table.Cell().AlignRight().Text($"KES {openingArrears:N2}");

                            table.Cell().Text("Invoices Issued:").Bold();
                            table.Cell().AlignRight().Text($"KES {invoiceTotal:N2}");

                            table.Cell().Text("Payments Collected:").Bold();
                            table.Cell().AlignRight().Text($"KES {paymentsTotal:N2}");

                            table.Cell().Text("Expenses:").Bold();
                            table.Cell().AlignRight().Text($"KES {expensesTotal:N2}");

                            table.Cell().Text("Closing Arrears:").Bold();
                            table.Cell().AlignRight().Text($"KES {closingArrears:N2}");
                        });

                        if (periodInvoices.Any())
                        {
                            column.Item().PaddingTop(20).Text("INVOICES").Bold().FontSize(14);
                            column.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(2);
                                    cols.RelativeColumn(2);
                                    cols.RelativeColumn(1);
                                    cols.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Tenant").Bold();
                                    header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Property/Unit").Bold();
                                    header.Cell().Background(Colors.Blue.Lighten3).Padding(5).AlignRight().Text("Amount").Bold();
                                    header.Cell().Background(Colors.Blue.Lighten3).Padding(5).AlignRight().Text("Balance").Bold();
                                });

                                foreach (var invoice in periodInvoices)
                                {
                                    table.Cell().Padding(5).Text(invoice.Tenant.FullName);
                                    table.Cell().Padding(5).Text($"{invoice.Property.Name} - {invoice.Unit.UnitNumber}");
                                    table.Cell().Padding(5).AlignRight().Text($"KES {invoice.Amount + invoice.OpeningBalance:N2}");
                                    table.Cell().Padding(5).AlignRight().Text($"KES {invoice.Balance:N2}");
                                }
                            });
                        }

                        if (utilityTotals.Any())
                        {
                            column.Item().PaddingTop(20).Text("UTILITIES BREAKDOWN").Bold().FontSize(14);
                            column.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(2);
                                    cols.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Green.Lighten3).Padding(5).Text("Utility").Bold();
                                    header.Cell().Background(Colors.Green.Lighten3).Padding(5).AlignRight().Text("Total").Bold();
                                });

                                foreach (var utility in utilityTotals)
                                {
                                    table.Cell().Padding(5).Text(utility.UtilityName);
                                    table.Cell().Padding(5).AlignRight().Text($"KES {utility.Total:N2}");
                                }
                            });
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Darken1))
                    .Text(text =>
                    {
                        text.Span("Generated on ");
                        text.Span(DateTime.UtcNow.ToString("dd MMM yyyy HH:mm")).Bold();
                    });
            });
        });

        _logger.LogInformation("Generated owner statement for Landlord {LandlordId} {Year}-{Month}", landlordId, year, month);
        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateRentRollAsync(int? landlordId = null, IReadOnlyCollection<int>? propertyIds = null, int? organizationId = null)
    {
        try
        {
            var unitsQuery = _context.Units
                .Include(u => u.Property)
                .Include(u => u.Tenants)
                .Where(u => u.IsActive)
                .AsQueryable();

            if (organizationId.HasValue)
            {
                unitsQuery = unitsQuery.Where(u => u.Property.OrganizationId == organizationId.Value);
            }

            if (landlordId.HasValue)
            {
                unitsQuery = unitsQuery.Where(u => u.Property.LandlordId == landlordId.Value);
            }
            else if (propertyIds != null && propertyIds.Count > 0)
            {
                unitsQuery = unitsQuery.Where(u => propertyIds.Contains(u.PropertyId));
            }

            var units = await unitsQuery
                .OrderBy(u => u.Property.Name)
                .ThenBy(u => u.UnitNumber)
                .ToListAsync();

            var totalUnits = units.Count;
            var occupiedUnits = units.Count(u => u.Tenants.Any(t => t.IsActive));

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Height(90)
                        .Background(Colors.Blue.Lighten3)
                        .Padding(20)
                        .Column(column =>
                        {
                            column.Item().Text("RENT ROLL")
                                .FontSize(22)
                                .FontColor(Colors.Blue.Darken2)
                                .Bold();

                            column.Item().Text($"Total Units: {totalUnits} | Occupied: {occupiedUnits} | Vacant: {totalUnits - occupiedUnits}")
                                .FontSize(12)
                                .FontColor(Colors.Grey.Darken1);
                        });

                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Item().Text("UNIT DETAILS").Bold().FontSize(12);
                            column.Item().PaddingTop(8).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2); // Property
                                    columns.RelativeColumn(1); // Unit
                                    columns.RelativeColumn(2); // Tenant
                                    columns.RelativeColumn(1); // Status
                                    columns.RelativeColumn(1); // Rent
                                    columns.RelativeColumn(1); // Due Day
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Property").Bold();
                                    header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Unit").Bold();
                                    header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Tenant").Bold();
                                    header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Status").Bold();
                                    header.Cell().Background(Colors.Blue.Lighten3).Padding(5).AlignRight().Text("Monthly Rent").Bold();
                                    header.Cell().Background(Colors.Blue.Lighten3).Padding(5).AlignRight().Text("Due Day").Bold();
                                });

                                foreach (var unit in units)
                                {
                                    var activeTenant = unit.Tenants.FirstOrDefault(t => t.IsActive);
                                    var status = activeTenant != null ? "Occupied" : "Vacant";
                                    var rentAmount = activeTenant?.MonthlyRent ?? unit.MonthlyRent;
                                    var rentDueDay = activeTenant?.RentDueDay;

                                    table.Cell().Padding(5).Text(unit.Property.Name);
                                    table.Cell().Padding(5).Text(unit.UnitNumber);
                                    table.Cell().Padding(5).Text(activeTenant?.FullName ?? "Vacant");
                                    table.Cell().Padding(5).Text(status);
                                    table.Cell().Padding(5).AlignRight().Text($"KES {rentAmount:N2}");
                                    table.Cell().Padding(5).AlignRight().Text(rentDueDay?.ToString() ?? "-");
                                }
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Darken1))
                        .Text(text =>
                        {
                            text.Span("Generated on ");
                            text.Span(DateTime.UtcNow.ToString("dd MMM yyyy HH:mm")).Bold();
                        });
                });
            });

            _logger.LogInformation("Generated rent roll report");
            return document.GeneratePdf();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating rent roll report");
            throw;
        }
    }
}
