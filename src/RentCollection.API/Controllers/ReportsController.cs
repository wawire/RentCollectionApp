using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Application.Authorization;
using RentCollection.Domain.Enums;
using System.Security.Claims;

namespace RentCollection.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportsService _reportsService;

        public ReportsController(IReportsService reportsService)
        {
            _reportsService = reportsService;
        }

        /// <summary>
        /// Generate Profit & Loss Report for a given period
        /// </summary>
        /// <param name="startDate">Start date of the reporting period</param>
        /// <param name="endDate">End date of the reporting period</param>
        [HttpGet("profit-loss")]
        [PermissionAuthorize(Permission.ViewReports)]
        public async Task<IActionResult> GetProfitLossReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Only landlords see their own properties, admins see all
            int? landlordId = userRole == "Landlord" ? currentUserId : null;

            var result = await _reportsService.GenerateProfitLossReportAsync(startDate, endDate, landlordId);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { data = result.Data });
        }

        /// <summary>
        /// Generate Arrears Report showing all tenants with overdue payments
        /// </summary>
        [HttpGet("arrears")]
        [PermissionAuthorize(Permission.ViewReports)]
        public async Task<IActionResult> GetArrearsReport()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Only landlords see their own properties, admins see all
            int? landlordId = userRole == "Landlord" ? currentUserId : null;

            var result = await _reportsService.GenerateArrearsReportAsync(landlordId);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { data = result.Data });
        }

        /// <summary>
        /// Generate Occupancy Report showing vacancy rates and potential revenue
        /// </summary>
        [HttpGet("occupancy")]
        [PermissionAuthorize(Permission.ViewReports)]
        public async Task<IActionResult> GetOccupancyReport()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Only landlords see their own properties, admins see all
            int? landlordId = userRole == "Landlord" ? currentUserId : null;

            var result = await _reportsService.GenerateOccupancyReportAsync(landlordId);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { data = result.Data });
        }
    }
}
