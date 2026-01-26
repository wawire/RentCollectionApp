using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.API.Controllers;

/// <summary>
/// Dashboard and reporting endpoints
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Get dashboard statistics
    /// </summary>
    /// <returns>Real-time dashboard statistics including properties, units, tenants, and financial data</returns>
    [HttpGet("stats")]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager,Caretaker,Accountant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetStats()
    {
        var result = await _dashboardService.GetDashboardStatsAsync();

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get monthly financial report
    /// </summary>
    /// <param name="year">Year for the report (e.g., 2024)</param>
    /// <returns>12-month financial report with rent collection data</returns>
    [HttpGet("monthly-report/{year}")]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager,Caretaker,Accountant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMonthlyReport(int year)
    {
        var result = await _dashboardService.GetMonthlyReportAsync(year);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}


