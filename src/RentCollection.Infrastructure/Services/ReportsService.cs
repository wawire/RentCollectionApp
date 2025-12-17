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

        public async Task<ServiceResult<ProfitLossReportDto>> GenerateProfitLossReportAsync(DateTime startDate, DateTime endDate, int? landlordId = null)
        {
            try
            {
                // Get all properties for the landlord (or all if landlordId is null)
                var propertiesQuery = _context.Properties
                    .Include(p => p.Units)
                        .ThenInclude(u => u.Tenants)
                            .ThenInclude(t => t.Payments)
                    .AsQueryable();

                if (landlordId.HasValue)
                {
                    propertiesQuery = propertiesQuery.Where(p => p.LandlordId == landlordId.Value);
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

                if (landlordId.HasValue)
                {
                    expensesQuery = expensesQuery.Where(e => e.LandlordId == landlordId.Value);
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

        public async Task<ServiceResult<ArrearsReportDto>> GenerateArrearsReportAsync(int? landlordId = null)
        {
            try
            {
                var today = DateTime.UtcNow.Date;

                // Get all tenants with overdue payments
                var tenantsQuery = _context.Tenants
                    .Include(t => t.Unit)
                        .ThenInclude(u => u.Property)
                    .Include(t => t.Payments)
                    .Where(t => t.IsActive)
                    .AsQueryable();

                if (landlordId.HasValue)
                {
                    tenantsQuery = tenantsQuery.Where(t => t.Unit.Property.LandlordId == landlordId.Value);
                }

                var tenants = await tenantsQuery.ToListAsync();

                var report = new ArrearsReportDto
                {
                    ReportDate = today
                };

                foreach (var tenant in tenants)
                {
                    var overduePayments = tenant.Payments
                        .Where(p => p.Status == PaymentStatus.Pending && p.DueDate.Date < today)
                        .OrderBy(p => p.DueDate)
                        .ToList();

                    if (overduePayments.Any())
                    {
                        var tenantArrears = new TenantArrearsDto
                        {
                            TenantId = tenant.Id,
                            TenantName = tenant.FullName,
                            Email = tenant.Email,
                            PhoneNumber = tenant.PhoneNumber,
                            PropertyName = tenant.Unit.Property.Name,
                            UnitNumber = tenant.Unit.UnitNumber,
                            TotalArrears = overduePayments.Sum(p => p.Amount),
                            TotalLateFees = overduePayments.Sum(p => p.LateFeeAmount),
                            OldestOverdueDate = overduePayments.First().DueDate,
                            DaysOverdue = (today - overduePayments.First().DueDate).Days
                        };

                        tenantArrears.TotalOutstanding = tenantArrears.TotalArrears + tenantArrears.TotalLateFees;

                        foreach (var payment in overduePayments)
                        {
                            tenantArrears.OverduePayments.Add(new OverduePaymentDto
                            {
                                PaymentId = payment.Id,
                                DueDate = payment.DueDate,
                                DaysOverdue = (today - payment.DueDate).Days,
                                Amount = payment.Amount,
                                LateFee = payment.LateFeeAmount,
                                TotalDue = payment.Amount + payment.LateFeeAmount,
                                PeriodStart = payment.PeriodStart.ToString("MMM dd, yyyy"),
                                PeriodEnd = payment.PeriodEnd.ToString("MMM dd, yyyy")
                            });
                        }

                        report.TenantArrears.Add(tenantArrears);
                    }
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

                if (landlordId.HasValue)
                {
                    propertiesQuery = propertiesQuery.Where(p => p.LandlordId == landlordId.Value);
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

        public async Task<ServiceResult<OccupancyReportDto>> GenerateOccupancyReportAsync(int? landlordId = null)
        {
            try
            {
                var today = DateTime.UtcNow.Date;

                var propertiesQuery = _context.Properties
                    .Include(p => p.Units)
                        .ThenInclude(u => u.Tenants)
                    .AsQueryable();

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
