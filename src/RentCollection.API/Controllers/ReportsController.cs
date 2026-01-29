using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Application.Authorization;
using RentCollection.Domain.Enums;

namespace RentCollection.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager,Accountant")]
    [Authorize(Policy = Policies.RequireVerifiedUser)]
    [Authorize(Policy = Policies.RequireActiveOrganization)]
    public class ReportsController : ControllerBase
    {
        private readonly IReportsService _reportsService;
        private readonly IPdfService _pdfService;
        private readonly IPaymentService _paymentService;
        private readonly ICurrentUserService _currentUserService;

        public ReportsController(
            IReportsService reportsService,
            IPdfService pdfService,
            IPaymentService paymentService,
            ICurrentUserService currentUserService)
        {
            _reportsService = reportsService;
            _pdfService = pdfService;
            _paymentService = paymentService;
            _currentUserService = currentUserService;
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
            var scope = await ResolveReportScopeAsync();
            if (!scope.IsAllowed)
            {
                return Forbid();
            }

            var result = await _reportsService.GenerateProfitLossReportAsync(startDate, endDate, scope.LandlordId, scope.PropertyIds, scope.OrganizationId);

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
            var scope = await ResolveReportScopeAsync();
            if (!scope.IsAllowed)
            {
                return Forbid();
            }

            var result = await _reportsService.GenerateArrearsReportAsync(scope.LandlordId, scope.PropertyIds, scope.OrganizationId);

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
            var scope = await ResolveReportScopeAsync();
            if (!scope.IsAllowed)
            {
                return Forbid();
            }

            var result = await _reportsService.GenerateOccupancyReportAsync(scope.LandlordId, scope.PropertyIds, scope.OrganizationId);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { data = result.Data });
        }

        /// <summary>
        /// Download monthly financial report PDF
        /// </summary>
        [HttpGet("monthly-report/{year}/{month}")]
        [PermissionAuthorize(Permission.ViewReports)]
        public async Task<IActionResult> DownloadMonthlyReport(int year, int month)
        {
            var scope = await ResolveReportScopeAsync();
            if (!scope.IsAllowed)
            {
                return Forbid();
            }

            var pdfBytes = await _pdfService.GenerateMonthlyReportAsync(year, month, scope.LandlordId, scope.PropertyIds, scope.OrganizationId);
            var fileName = $"Monthly_Report_{year}{month:00}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        /// <summary>
        /// Preview monthly financial report PDF
        /// </summary>
        [HttpGet("monthly-report/{year}/{month}/preview")]
        [PermissionAuthorize(Permission.ViewReports)]
        public async Task<IActionResult> PreviewMonthlyReport(int year, int month)
        {
            var scope = await ResolveReportScopeAsync();
            if (!scope.IsAllowed)
            {
                return Forbid();
            }

            var pdfBytes = await _pdfService.GenerateMonthlyReportAsync(year, month, scope.LandlordId, scope.PropertyIds, scope.OrganizationId);
            Response.Headers["Content-Disposition"] = $"inline; filename=Monthly_Report_{year}{month:00}.pdf";
            return File(pdfBytes, "application/pdf");
        }

        /// <summary>
        /// Download tenant list PDF
        /// </summary>
        [HttpGet("tenant-list")]
        [PermissionAuthorize(Permission.ViewReports)]
        public async Task<IActionResult> DownloadTenantList()
        {
            var scope = await ResolveReportScopeAsync();
            if (!scope.IsAllowed)
            {
                return Forbid();
            }

            var pdfBytes = await _pdfService.GenerateTenantListAsync(scope.LandlordId, scope.PropertyIds, scope.OrganizationId);
            var fileName = $"Tenant_List_{DateTime.UtcNow:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        /// <summary>
        /// Preview tenant list PDF
        /// </summary>
        [HttpGet("tenant-list/preview")]
        [PermissionAuthorize(Permission.ViewReports)]
        public async Task<IActionResult> PreviewTenantList()
        {
            var scope = await ResolveReportScopeAsync();
            if (!scope.IsAllowed)
            {
                return Forbid();
            }

            var pdfBytes = await _pdfService.GenerateTenantListAsync(scope.LandlordId, scope.PropertyIds, scope.OrganizationId);
            Response.Headers["Content-Disposition"] = $"inline; filename=Tenant_List_{DateTime.UtcNow:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf");
        }

        /// <summary>
        /// Download rent roll PDF
        /// </summary>
        [HttpGet("rent-roll")]
        [PermissionAuthorize(Permission.ViewReports)]
        public async Task<IActionResult> DownloadRentRoll([FromQuery] int? propertyId = null)
        {
            var scope = await ResolveReportScopeAsync();
            if (!scope.IsAllowed)
            {
                return Forbid();
            }

            IReadOnlyCollection<int>? propertyIds = scope.PropertyIds;
            if (propertyId.HasValue)
            {
                propertyIds = new List<int> { propertyId.Value };
            }

            var pdfBytes = await _pdfService.GenerateRentRollAsync(scope.LandlordId, propertyIds, scope.OrganizationId);
            var fileName = propertyId.HasValue
                ? $"Rent_Roll_Property_{propertyId.Value}_{DateTime.UtcNow:yyyyMMdd}.pdf"
                : $"Rent_Roll_{DateTime.UtcNow:yyyyMMdd}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        /// <summary>
        /// Preview rent roll PDF
        /// </summary>
        [HttpGet("rent-roll/preview")]
        [PermissionAuthorize(Permission.ViewReports)]
        public async Task<IActionResult> PreviewRentRoll([FromQuery] int? propertyId = null)
        {
            var scope = await ResolveReportScopeAsync();
            if (!scope.IsAllowed)
            {
                return Forbid();
            }

            IReadOnlyCollection<int>? propertyIds = scope.PropertyIds;
            if (propertyId.HasValue)
            {
                propertyIds = new List<int> { propertyId.Value };
            }

            var pdfBytes = await _pdfService.GenerateRentRollAsync(scope.LandlordId, propertyIds, scope.OrganizationId);
            Response.Headers["Content-Disposition"] = "inline; filename=Rent_Roll.pdf";
            return File(pdfBytes, "application/pdf");
        }

        /// <summary>
        /// Download payment receipt PDF
        /// </summary>
        [HttpGet("payment-receipt/{paymentId}")]
        [PermissionAuthorize(Permission.ViewReports)]
        public async Task<IActionResult> DownloadPaymentReceipt(int paymentId)
        {
            var accessCheck = await _paymentService.GetPaymentByIdAsync(paymentId);
            if (!accessCheck.IsSuccess)
            {
                var message = accessCheck.ErrorMessage;
                if (message.Contains("permission", StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid();
                }

                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(new { message });
                }

                return BadRequest(new { message });
            }

            var pdfBytes = await _pdfService.GeneratePaymentReceiptAsync(paymentId);
            var fileName = $"Payment_Receipt_{paymentId}_{DateTime.UtcNow:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        /// <summary>
        /// Preview payment receipt PDF
        /// </summary>
        [HttpGet("payment-receipt/{paymentId}/preview")]
        [PermissionAuthorize(Permission.ViewReports)]
        public async Task<IActionResult> PreviewPaymentReceipt(int paymentId)
        {
            var accessCheck = await _paymentService.GetPaymentByIdAsync(paymentId);
            if (!accessCheck.IsSuccess)
            {
                var message = accessCheck.ErrorMessage;
                if (message.Contains("permission", StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid();
                }

                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(new { message });
                }

                return BadRequest(new { message });
            }

            var pdfBytes = await _pdfService.GeneratePaymentReceiptAsync(paymentId);
            Response.Headers["Content-Disposition"] = $"inline; filename=Payment_Receipt_{paymentId}.pdf";
            return File(pdfBytes, "application/pdf");
        }

        private async Task<(bool IsAllowed, int? LandlordId, IReadOnlyCollection<int>? PropertyIds, int? OrganizationId)> ResolveReportScopeAsync()
        {
            if (_currentUserService.IsPlatformAdmin)
            {
                return (true, null, null, null);
            }

            if (_currentUserService.IsLandlord)
            {
                return (_currentUserService.UserIdInt.HasValue, _currentUserService.UserIdInt, null, _currentUserService.OrganizationId);
            }

            if (_currentUserService.IsAccountant || _currentUserService.IsManager || _currentUserService.IsCaretaker)
            {
                var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
                if (assignedPropertyIds.Count == 0)
                {
                    return (false, null, null, null);
                }

                return (true, null, assignedPropertyIds, _currentUserService.OrganizationId);
            }

            return (false, null, null, null);
        }
    }
}

