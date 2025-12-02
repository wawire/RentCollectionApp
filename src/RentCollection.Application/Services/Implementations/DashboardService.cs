using Microsoft.Extensions.Logging;
using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Dashboard;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Interfaces;
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
            var properties = await _propertyRepository.GetAllAsync();
            var units = await _unitRepository.GetAllAsync();
            var tenants = await _tenantRepository.GetAllAsync();
            var activeTenants = await _tenantRepository.GetActiveTenantsAsync();

            // Filter data by LandlordId (unless SystemAdmin)
            if (!_currentUserService.IsSystemAdmin)
            {
                var landlordIdStr = _currentUserService.IsLandlord
                    ? _currentUserService.UserId
                    : _currentUserService.LandlordId;

                if (int.TryParse(landlordIdStr, out var landlordId))
                {
                    properties = properties.Where(p => p.LandlordId == landlordId).ToList();
                    units = units.Where(u => u.Property?.LandlordId == landlordId).ToList();
                    tenants = tenants.Where(t => t.Unit?.Property?.LandlordId == landlordId).ToList();
                    activeTenants = activeTenants.Where(t => t.Unit?.Property?.LandlordId == landlordId).ToList();
                }
            }

            // Calculate current month's payments
            var currentMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);
            var allPayments = await _paymentRepository.GetPaymentsByDateRangeAsync(currentMonthStart, currentMonthEnd);

            // Filter payments by LandlordId (unless SystemAdmin)
            var currentMonthPayments = allPayments;
            if (!_currentUserService.IsSystemAdmin)
            {
                var landlordIdStr = _currentUserService.IsLandlord
                    ? _currentUserService.UserId
                    : _currentUserService.LandlordId;

                if (int.TryParse(landlordIdStr, out var landlordId))
                {
                    currentMonthPayments = allPayments.Where(p => p.Tenant?.Unit?.Property?.LandlordId == landlordId).ToList();
                }
            }

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
                    .Sum(p => p.Amount)
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

                // Filter payments by LandlordId (unless SystemAdmin)
                var monthlyPayments = allMonthlyPayments;
                if (!_currentUserService.IsSystemAdmin)
                {
                    var landlordIdStr = _currentUserService.IsLandlord
                        ? _currentUserService.UserId
                        : _currentUserService.LandlordId;

                    if (int.TryParse(landlordIdStr, out var landlordId))
                    {
                        monthlyPayments = allMonthlyPayments.Where(p => p.Tenant?.Unit?.Property?.LandlordId == landlordId).ToList();
                    }
                }

                // Get expected rent (active tenants for that month)
                var allActiveTenants = await _tenantRepository.GetActiveTenantsAsync();

                // Filter tenants by LandlordId (unless SystemAdmin)
                var activeTenants = allActiveTenants;
                if (!_currentUserService.IsSystemAdmin)
                {
                    var landlordIdStr = _currentUserService.IsLandlord
                        ? _currentUserService.UserId
                        : _currentUserService.LandlordId;

                    if (int.TryParse(landlordIdStr, out var landlordId))
                    {
                        activeTenants = allActiveTenants.Where(t => t.Unit?.Property?.LandlordId == landlordId).ToList();
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
