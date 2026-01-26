using Microsoft.EntityFrameworkCore;
using RentCollection.Application.Common;
using RentCollection.Application.DTOs.Reports;
using RentCollection.Application.Helpers;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Services
{
    public class ReportsService : IReportsService
    {
        private readonly ApplicationDbContext _context;

        public ReportsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<ProfitLossReportDto>> GenerateProfitLossReportAsync(DateTime startDate, DateTime endDate, int? landlordId = null, IReadOnlyCollection<int>? propertyIds = null, int? organizationId = null)
        {
            try
            {
                // Get all properties for the landlord (or all if landlordId is null)
                var propertiesQuery = _context.Properties
                    .Include(p => p.Units)
                        .ThenInclude(u => u.Tenants)
                            .ThenInclude(t => t.Payments)
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
                else if (propertyIds != null && propertyIds.Count > 0)
                {
                    propertiesQuery = propertiesQuery.Where(p => propertyIds.Contains(p.Id));
                }

                var properties = await propertiesQuery.ToListAsync();

                var report = new ProfitLossReportDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    Period = $"{startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}"
                };

                foreach (var property in properties)
                {
                    var propertyReport = new PropertyProfitLossDto
                    {
                        PropertyId = property.Id,
                        PropertyName = property.Name,
                        TotalUnits = property.Units.Count,
                        OccupiedUnits = property.Units.Count(u => u.Tenants.Any(t => t.IsActive))
                    };

                    propertyReport.OccupancyRate = propertyReport.TotalUnits > 0
                        ? (decimal)propertyReport.OccupiedUnits / propertyReport.TotalUnits * 100
                        : 0;

                    // Calculate rent collected and expected for the period
                    foreach (var unit in property.Units)
                    {
                        var activeTenant = unit.Tenants.FirstOrDefault(t => t.IsActive);
                        if (activeTenant != null)
                        {
                            // Rent expected for the period
                            var monthsInPeriod = ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month + 1;
                            propertyReport.RentExpected += activeTenant.MonthlyRent * monthsInPeriod;

                            // Rent collected (completed payments in the period)
                            var completedPayments = activeTenant.Payments
                                .Where(p => p.Status == PaymentStatus.Completed &&
                                           p.PaymentDate >= startDate &&
                                           p.PaymentDate <= endDate)
                                .ToList();

                            propertyReport.RentCollected += completedPayments.Sum(p => p.Amount);
                            propertyReport.LateFees += completedPayments.Sum(p => p.LateFeeAmount);
                        }
                    }

                    propertyReport.CollectionRate = propertyReport.RentExpected > 0
                        ? propertyReport.RentCollected / propertyReport.RentExpected * 100
                        : 0;

                    propertyReport.TotalIncome = propertyReport.RentCollected + propertyReport.LateFees;

                    // Get expenses for this property in the period
                    var propertyExpenses = await _context.Expenses
                        .Where(e => e.PropertyId == property.Id &&
                                   e.ExpenseDate >= startDate &&
                                   e.ExpenseDate <= endDate)
                        .SumAsync(e => e.Amount);

                    propertyReport.Expenses = propertyExpenses;

                    propertyReport.NetProfit = propertyReport.TotalIncome - propertyReport.Expenses;

                    report.PropertiesBreakdown.Add(propertyReport);
                }

                // Calculate totals
                report.TotalRentCollected = report.PropertiesBreakdown.Sum(p => p.RentCollected);
                report.TotalRentExpected = report.PropertiesBreakdown.Sum(p => p.RentExpected);
                report.CollectionRate = report.TotalRentExpected > 0
                    ? report.TotalRentCollected / report.TotalRentExpected * 100
                    : 0;
                report.LateFees = report.PropertiesBreakdown.Sum(p => p.LateFees);
                report.TotalIncome = report.TotalRentCollected + report.LateFees;
                report.TotalExpenses = report.PropertiesBreakdown.Sum(p => p.Expenses);

                // Get expenses by category for the entire period
                var expensesQuery = _context.Expenses
                    .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate);

                if (organizationId.HasValue)
                {
                    expensesQuery = expensesQuery.Where(e => e.Property.OrganizationId == organizationId.Value);
                }

                if (landlordId.HasValue)
                {
                    expensesQuery = expensesQuery.Where(e => e.LandlordId == landlordId.Value);
                }
                else if (propertyIds != null && propertyIds.Count > 0)
                {
                    expensesQuery = expensesQuery.Where(e => propertyIds.Contains(e.PropertyId));
                }

                var expensesByCategory = await expensesQuery
                    .GroupBy(e => e.Category)
                    .Select(g => new { Category = g.Key.ToString(), Total = g.Sum(e => e.Amount) })
                    .ToListAsync();

                foreach (var item in expensesByCategory)
                {
                    report.ExpensesByCategory[item.Category] = item.Total;

                    // Also populate the specific category fields for backward compatibility
                    switch (item.Category)
                    {
                        case "Maintenance":
                            report.MaintenanceExpenses = item.Total;
                            break;
                        case "Utilities":
                            report.UtilitiesExpenses = item.Total;
                            break;
                        case "Management":
                            report.PropertyManagementFees = item.Total;
                            break;
                        case "Insurance":
                        case "PropertyTax":
                            report.TaxesAndInsurance += item.Total;
                            break;
                        default:
                            report.OtherExpenses += item.Total;
                            break;
                    }
                }

                report.NetProfit = report.TotalIncome - report.TotalExpenses;
                report.ProfitMargin = report.TotalIncome > 0
                    ? report.NetProfit / report.TotalIncome * 100
                    : 0;

                return ServiceResult<ProfitLossReportDto>.Success(report);
            }
            catch (Exception ex)
            {
                return ServiceResult<ProfitLossReportDto>.Failure($"Failed to generate profit/loss report: {ex.Message}");
            }
        }

        public async Task<ServiceResult<ArrearsReportDto>> GenerateArrearsReportAsync(int? landlordId = null, IReadOnlyCollection<int>? propertyIds = null, int? organizationId = null)
        {
            try
            {
                var today = DateTime.UtcNow.Date;

                // Use invoices (with allocations) as the source of truth for arrears
                var invoicesQuery = _context.Invoices
                    .Include(i => i.Tenant)
                        .ThenInclude(t => t.Unit)
                            .ThenInclude(u => u.Property)
                    .Include(i => i.Allocations)
                    .Include(i => i.LineItems)
                    .Where(i => i.Status != InvoiceStatus.Void && i.Tenant.Status == TenantStatus.Active)
                    .AsQueryable();

                if (organizationId.HasValue)
                {
                    invoicesQuery = invoicesQuery.Where(i => i.Property.OrganizationId == organizationId.Value);
                }

                if (landlordId.HasValue)
                {
                    invoicesQuery = invoicesQuery.Where(i => i.LandlordId == landlordId.Value);
                }
                else if (propertyIds != null && propertyIds.Count > 0)
                {
                    invoicesQuery = invoicesQuery.Where(i => propertyIds.Contains(i.PropertyId));
                }

                var invoices = await invoicesQuery.ToListAsync();

                var report = new ArrearsReportDto
                {
                    ReportDate = today
                };

                var invoiceSummaries = invoices
                    .Select(invoice =>
                    {
                        var allocated = invoice.Allocations.Sum(a => a.Amount);
                        var balance = InvoiceStatusCalculator.CalculateBalance(invoice, allocated);
                        var lateFee = invoice.LineItems
                            .Where(li => li.LineItemType == InvoiceLineItemType.LateFee)
                            .Sum(li => li.Amount);
                        return new { Invoice = invoice, Balance = balance, LateFee = lateFee };
                    })
                    .Where(summary => summary.Balance > 0 && summary.Invoice.DueDate.Date < today)
                    .ToList();

                foreach (var tenantGroup in invoiceSummaries.GroupBy(s => s.Invoice.TenantId))
                {
                    var first = tenantGroup.First();
                    var tenant = first.Invoice.Tenant;
                    if (tenant?.Unit?.Property == null)
                    {
                        continue;
                    }

                    var tenantArrears = new TenantArrearsDto
                    {
                        TenantId = tenant.Id,
                        TenantName = tenant.FullName,
                        Email = tenant.Email,
                        PhoneNumber = tenant.PhoneNumber,
                        PropertyName = tenant.Unit.Property.Name,
                        UnitNumber = tenant.Unit.UnitNumber,
                        TotalArrears = tenantGroup.Sum(i => Math.Max(0, i.Balance - i.LateFee)),
                        TotalLateFees = tenantGroup.Sum(i => i.LateFee),
                        OldestOverdueDate = tenantGroup.OrderBy(i => i.Invoice.DueDate).First().Invoice.DueDate,
                        DaysOverdue = (today - tenantGroup.OrderBy(i => i.Invoice.DueDate).First().Invoice.DueDate).Days
                    };

                    tenantArrears.TotalOutstanding = tenantArrears.TotalArrears + tenantArrears.TotalLateFees;

                    foreach (var item in tenantGroup.OrderBy(i => i.Invoice.DueDate))
                    {
                        var baseAmount = Math.Max(0, item.Balance - item.LateFee);
                        tenantArrears.OverduePayments.Add(new OverduePaymentDto
                        {
                            PaymentId = item.Invoice.Id,
                            DueDate = item.Invoice.DueDate,
                            DaysOverdue = (today - item.Invoice.DueDate).Days,
                            Amount = baseAmount,
                            LateFee = item.LateFee,
                            TotalDue = item.Balance,
                            PeriodStart = item.Invoice.PeriodStart.ToString("MMM dd, yyyy"),
                            PeriodEnd = item.Invoice.PeriodEnd.ToString("MMM dd, yyyy")
                        });
                    }

                    report.TenantArrears.Add(tenantArrears);
                }

                // Calculate totals
                report.TotalArrears = report.TenantArrears.Sum(t => t.TotalArrears);
                report.TotalLateFees = report.TenantArrears.Sum(t => t.TotalLateFees);
                report.TotalOutstanding = report.TotalArrears + report.TotalLateFees;
                report.TotalTenantsInArrears = report.TenantArrears.Count;

                // Group by property
                var propertiesQuery = _context.Properties
                    .Include(p => p.Units)
                        .ThenInclude(u => u.Tenants)
                            .ThenInclude(t => t.Payments)
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

                foreach (var property in properties)
                {
                    var propertyTenants = report.TenantArrears.Where(t => t.PropertyName == property.Name).ToList();

                    if (propertyTenants.Any())
                    {
                        var propertyArrears = new PropertyArrearsDto
                        {
                            PropertyId = property.Id,
                            PropertyName = property.Name,
                            TotalArrears = propertyTenants.Sum(t => t.TotalArrears),
                            TotalLateFees = propertyTenants.Sum(t => t.TotalLateFees),
                            TotalOutstanding = propertyTenants.Sum(t => t.TotalOutstanding),
                            TenantsInArrears = propertyTenants.Count,
                            TotalTenants = property.Units.Count(u => u.Tenants.Any(t => t.IsActive))
                        };

                        propertyArrears.ArrearsRate = propertyArrears.TotalTenants > 0
                            ? (decimal)propertyArrears.TenantsInArrears / propertyArrears.TotalTenants * 100
                            : 0;

                        report.PropertiesBreakdown.Add(propertyArrears);
                    }
                }

                return ServiceResult<ArrearsReportDto>.Success(report);
            }
            catch (Exception ex)
            {
                return ServiceResult<ArrearsReportDto>.Failure($"Failed to generate arrears report: {ex.Message}");
            }
        }

        public async Task<ServiceResult<OccupancyReportDto>> GenerateOccupancyReportAsync(int? landlordId = null, IReadOnlyCollection<int>? propertyIds = null, int? organizationId = null)
        {
            try
            {
                var today = DateTime.UtcNow.Date;

                var propertiesQuery = _context.Properties
                    .Include(p => p.Units)
                        .ThenInclude(u => u.Tenants)
                    .AsQueryable();

                if (organizationId.HasValue)
                {
                    propertiesQuery = propertiesQuery.Where(p => p.OrganizationId == organizationId.Value);
                }

                if (landlordId.HasValue)
                {
                    propertiesQuery = propertiesQuery.Where(p => p.LandlordId == landlordId.Value);
                }

                var properties = await propertiesQuery.ToListAsync();

                var report = new OccupancyReportDto
                {
                    ReportDate = today,
                    TotalProperties = properties.Count
                };

                foreach (var property in properties)
                {
                    var propertyReport = new PropertyOccupancyDto
                    {
                        PropertyId = property.Id,
                        PropertyName = property.Name,
                        PropertyType = "Residential", // Property entity doesn't have PropertyType
                        TotalUnits = property.Units.Count
                    };

                    foreach (var unit in property.Units)
                    {
                        var activeTenant = unit.Tenants.FirstOrDefault(t => t.IsActive);
                        if (activeTenant != null)
                        {
                            propertyReport.OccupiedUnits++;
                            propertyReport.ActualMonthlyRevenue += activeTenant.MonthlyRent;
                        }
                        else
                        {
                            propertyReport.VacantUnits++;
                            var vacantUnit = new VacantUnitDto
                            {
                                UnitId = unit.Id,
                                UnitNumber = unit.UnitNumber,
                                UnitType = "Standard", // Unit entity doesn't have UnitType property
                                RentAmount = unit.MonthlyRent,
                                DaysVacant = 0, // Would need to track this in the database
                                LastOccupiedDate = null // Would need to track this
                            };
                            propertyReport.VacantUnitsList.Add(vacantUnit);
                        }

                        propertyReport.PotentialMonthlyRevenue += unit.MonthlyRent;
                    }

                    propertyReport.OccupancyRate = propertyReport.TotalUnits > 0
                        ? (decimal)propertyReport.OccupiedUnits / propertyReport.TotalUnits * 100
                        : 0;

                    propertyReport.VacancyLoss = propertyReport.PotentialMonthlyRevenue - propertyReport.ActualMonthlyRevenue;

                    report.PropertiesBreakdown.Add(propertyReport);
                }

                // Calculate totals
                report.TotalUnits = report.PropertiesBreakdown.Sum(p => p.TotalUnits);
                report.OccupiedUnits = report.PropertiesBreakdown.Sum(p => p.OccupiedUnits);
                report.VacantUnits = report.PropertiesBreakdown.Sum(p => p.VacantUnits);
                report.OverallOccupancyRate = report.TotalUnits > 0
                    ? (decimal)report.OccupiedUnits / report.TotalUnits * 100
                    : 0;
                report.PotentialMonthlyRevenue = report.PropertiesBreakdown.Sum(p => p.PotentialMonthlyRevenue);
                report.ActualMonthlyRevenue = report.PropertiesBreakdown.Sum(p => p.ActualMonthlyRevenue);
                report.VacancyLoss = report.PotentialMonthlyRevenue - report.ActualMonthlyRevenue;

                return ServiceResult<OccupancyReportDto>.Success(report);
            }
            catch (Exception ex)
            {
                return ServiceResult<OccupancyReportDto>.Failure($"Failed to generate occupancy report: {ex.Message}");
            }
        }
    }
}
