using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Dashboard;

namespace RentCollection.Application.Services.Interfaces;

public interface IDashboardService
{
    Task<Result<DashboardStatsDto>> GetDashboardStatsAsync();
    Task<Result<IEnumerable<MonthlyReportDto>>> GetMonthlyReportAsync(int year);
}
