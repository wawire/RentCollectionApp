using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _dashboardService.GetDashboardStatsAsync();

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("monthly-report/{year}")]
    public async Task<IActionResult> GetMonthlyReport(int year)
    {
        var result = await _dashboardService.GetMonthlyReportAsync(year);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}
