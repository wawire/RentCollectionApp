using Microsoft.Extensions.Logging;
using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Dashboard;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUnitRepository _unitRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<DashboardService> _logger;
    private readonly ICurrentUserService _currentUserService;

    public DashboardService(
        IPropertyRepository propertyRepository,
        IUnitRepository unitRepository,
        ITenantRepository tenantRepository,
        IPaymentRepository paymentRepository,
        ILogger<DashboardService> logger,
        ICurrentUserService currentUserService)
    {
        _propertyRepository = propertyRepository;
        _unitRepository = unitRepository;
        _tenantRepository = tenantRepository;
        _paymentRepository = paymentRepository;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<DashboardStatsDto>> GetDashboardStatsAsync()
    {
        try
        {
            // Tenants do not have access to dashboard statistics
            if (_currentUserService.IsTenant)
            {
                return Result<DashboardStatsDto>.Failure("Tenants do not have permission to access dashboard statistics");
            }

            var properties = await _propertyRepository.GetAllAsync();
            var units = await _unitRepository.GetAllAsync();
            var tenants = await _tenantRepository.GetAllAsync();
            var activeTenants = await _tenantRepository.GetActiveTenantsAsync();

            // Filter data by scope (unless PlatformAdmin)
            if (!_currentUserService.IsPlatformAdmin)
            {
                if (_currentUserService.IsLandlord && _currentUserService.UserIdInt.HasValue)
                {
                    var landlordId = _currentUserService.UserIdInt.Value;
                    properties = properties.Where(p => p.LandlordId == landlordId).ToList();
                    units = units.Where(u => u.Property?.LandlordId == landlordId).ToList();
                    tenants = tenants.Where(t => t.Unit?.Property?.LandlordId == landlordId).ToList();
                    activeTenants = activeTenants.Where(t => t.Unit?.Property?.LandlordId == landlordId).ToList();
                }
                else if (_currentUserService.IsManager || _currentUserService.IsCaretaker || _currentUserService.IsAccountant)
                {
                    var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
                    properties = properties.Where(p => assignedPropertyIds.Contains(p.Id)).ToList();
                    units = units.Where(u => u.Property != null && assignedPropertyIds.Contains(u.Property.Id)).ToList();
                    tenants = tenants.Where(t => t.Unit?.Property != null && assignedPropertyIds.Contains(t.Unit.Property.Id)).ToList();
                    activeTenants = activeTenants.Where(t => t.Unit?.Property != null && assignedPropertyIds.Contains(t.Unit.Property.Id)).ToList();
                }
                else
                {
                    properties = new List<Property>();
                    units = new List<Unit>();
                    tenants = new List<Tenant>();
                    activeTenants = new List<Tenant>();
                }
            }

            // Calculate current month's payments
            var currentMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);
            var allPayments = await _paymentRepository.GetPaymentsByDateRangeAsync(currentMonthStart, currentMonthEnd);

            // Filter payments by scope (unless PlatformAdmin)
            var currentMonthPayments = allPayments;
            if (!_currentUserService.IsPlatformAdmin)
            {
                if (_currentUserService.IsLandlord && _currentUserService.UserIdInt.HasValue)
                {
                    var landlordId = _currentUserService.UserIdInt.Value;
                    currentMonthPayments = allPayments.Where(p => p.Tenant?.Unit?.Property?.LandlordId == landlordId).ToList();
                }
                else if (_currentUserService.IsManager || _currentUserService.IsCaretaker || _currentUserService.IsAccountant)
                {
                    var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
                    currentMonthPayments = allPayments.Where(p => p.Tenant?.Unit?.Property != null && assignedPropertyIds.Contains(p.Tenant.Unit.Property.Id)).ToList();
                }
                else
                {
                    currentMonthPayments = new List<Payment>();
                }
            }

            // Calculate payment status counts for occupied units
            var occupiedUnits = units.Where(u => u.IsOccupied).ToList();
            var unitsPaid = occupiedUnits.Count(u =>
                u.Tenants.Any(t => t.IsActive) &&
                u.Tenants.First(t => t.IsActive).Payments.Any(p =>
                    p.Status == PaymentStatus.Completed &&
                    p.PaymentDate >= DateTime.UtcNow.AddDays(-30)));

            var unitsPending = occupiedUnits.Count(u =>
                u.Tenants.Any(t => t.IsActive) &&
                u.Tenants.First(t => t.IsActive).Payments.Any(p => p.Status == PaymentStatus.Pending));

            var unitsOverdue = occupiedUnits.Count(u =>
                u.Tenants.Any(t => t.IsActive) &&
                !u.Tenants.First(t => t.IsActive).Payments.Any(p =>
                    p.Status == PaymentStatus.Completed &&
                    p.PaymentDate >= DateTime.UtcNow.AddDays(-30)) &&
                !u.Tenants.First(t => t.IsActive).Payments.Any(p => p.Status == PaymentStatus.Pending));

            var stats = new DashboardStatsDto
            {
                TotalProperties = properties.Count(),
                TotalUnits = units.Count(),
                OccupiedUnits = units.Count(u => u.IsOccupied),
                VacantUnits = units.Count(u => !u.IsOccupied),
                TotalTenants = tenants.Count(),
                ActiveTenants = activeTenants.Count(),
                TotalRentCollected = currentMonthPayments
                    .Where(p => p.Status == PaymentStatus.Completed)
                    .Sum(p => p.Amount),
                TotalRentExpected = activeTenants.Sum(t => t.MonthlyRent),
                PendingPayments = currentMonthPayments
                    .Where(p => p.Status == PaymentStatus.Pending)
                    .Sum(p => p.Amount),
                UnitsPaid = unitsPaid,
                UnitsOverdue = unitsOverdue,
                UnitsPending = unitsPending
            };

            // Calculate collection rate
            if (stats.TotalRentExpected > 0)
            {
                stats.CollectionRate = (stats.TotalRentCollected / stats.TotalRentExpected) * 100;
            }

            return Result<DashboardStatsDto>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard statistics");
            return Result<DashboardStatsDto>.Failure("An error occurred while retrieving dashboard statistics");
        }
    }

    public async Task<Result<IEnumerable<MonthlyReportDto>>> GetMonthlyReportAsync(int year)
    {
        try
        {
            // Tenants do not have access to monthly reports
            if (_currentUserService.IsTenant)
            {
                return Result<IEnumerable<MonthlyReportDto>>.Failure("Tenants do not have permission to access monthly reports");
            }

            if (year < 2000 || year > 2100)
            {
                return Result<IEnumerable<MonthlyReportDto>>.Failure("Invalid year specified");
            }

            var reports = new List<MonthlyReportDto>();

            for (int month = 1; month <= 12; month++)
            {
                var monthStart = new DateTime(year, month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                // Get payments for this month
                var allMonthlyPayments = await _paymentRepository.GetPaymentsByDateRangeAsync(monthStart, monthEnd);

                // Filter payments by scope (unless PlatformAdmin)
                var monthlyPayments = allMonthlyPayments;
                if (!_currentUserService.IsPlatformAdmin)
                {
                    if (_currentUserService.IsLandlord && _currentUserService.UserIdInt.HasValue)
                    {
                        var landlordId = _currentUserService.UserIdInt.Value;
                        monthlyPayments = allMonthlyPayments.Where(p => p.Tenant?.Unit?.Property?.LandlordId == landlordId).ToList();
                    }
                    else if (_currentUserService.IsManager || _currentUserService.IsCaretaker || _currentUserService.IsAccountant)
                    {
                        var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
                        monthlyPayments = allMonthlyPayments.Where(p => p.Tenant?.Unit?.Property != null && assignedPropertyIds.Contains(p.Tenant.Unit.Property.Id)).ToList();
                    }
                    else
                    {
                        monthlyPayments = new List<Payment>();
                    }
                }

                // Get expected rent (active tenants for that month)
                var allActiveTenants = await _tenantRepository.GetActiveTenantsAsync();

                // Filter tenants by scope (unless PlatformAdmin)
                var activeTenants = allActiveTenants;
                if (!_currentUserService.IsPlatformAdmin)
                {
                    if (_currentUserService.IsLandlord && _currentUserService.UserIdInt.HasValue)
                    {
                        var landlordId = _currentUserService.UserIdInt.Value;
                        activeTenants = allActiveTenants.Where(t => t.Unit?.Property?.LandlordId == landlordId).ToList();
                    }
                    else if (_currentUserService.IsManager || _currentUserService.IsCaretaker || _currentUserService.IsAccountant)
                    {
                        var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
                        activeTenants = allActiveTenants.Where(t => t.Unit?.Property != null && assignedPropertyIds.Contains(t.Unit.Property.Id)).ToList();
                    }
                    else
                    {
                        activeTenants = new List<Tenant>();
                    }
                }

                var tenantsActiveInMonth = activeTenants.Where(t =>
                    t.LeaseStartDate <= monthEnd &&
                    (!t.LeaseEndDate.HasValue || t.LeaseEndDate.Value >= monthStart));

                var totalRentCollected = monthlyPayments
                    .Where(p => p.Status == PaymentStatus.Completed)
                    .Sum(p => p.Amount);

                var totalRentExpected = tenantsActiveInMonth.Sum(t => t.MonthlyRent);

                var collectionRate = totalRentExpected > 0
                    ? (totalRentCollected / totalRentExpected) * 100
                    : 0;

                reports.Add(new MonthlyReportDto
                {
                    Year = year,
                    Month = month,
                    MonthName = new DateTime(year, month, 1).ToString("MMMM"),
                    TotalRentCollected = totalRentCollected,
                    TotalRentExpected = totalRentExpected,
                    NumberOfPayments = monthlyPayments.Count(p => p.Status == PaymentStatus.Completed),
                    CollectionRate = collectionRate
                });
            }

            return Result<IEnumerable<MonthlyReportDto>>.Success(reports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating monthly report for year {Year}", year);
            return Result<IEnumerable<MonthlyReportDto>>.Failure("An error occurred while generating the monthly report");
        }
    }
}

