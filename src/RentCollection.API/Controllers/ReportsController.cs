using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.API.Controllers;

/// <summary>
/// PDF reports and document generation endpoints
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IPdfService _pdfService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IPdfService pdfService, ILogger<ReportsController> logger)
    {
        _pdfService = pdfService;
        _logger = logger;
    }

    /// <summary>
    /// Generate and download payment receipt PDF
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <returns>PDF file download</returns>
    [HttpGet("payment-receipt/{paymentId}")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPaymentReceipt(int paymentId)
    {
        try
        {
            _logger.LogInformation("Generating payment receipt PDF for payment {PaymentId}", paymentId);

            var pdfBytes = await _pdfService.GeneratePaymentReceiptAsync(paymentId);
            var fileName = $"Payment-Receipt-{paymentId}-{DateTime.UtcNow:yyyyMMdd}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Payment {PaymentId} not found", paymentId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating payment receipt for payment {PaymentId}", paymentId);
            return StatusCode(500, new { message = "An error occurred while generating the payment receipt" });
        }
    }

    /// <summary>
    /// Generate and download monthly financial report PDF
    /// </summary>
    /// <param name="year">Year (e.g., 2024)</param>
    /// <param name="month">Month (1-12)</param>
    /// <returns>PDF file download</returns>
    [HttpGet("monthly-report/{year}/{month}")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMonthlyReport(int year, int month)
    {
        try
        {
            if (month < 1 || month > 12)
            {
                return BadRequest(new { message = "Month must be between 1 and 12" });
            }

            if (year < 2000 || year > DateTime.UtcNow.Year + 1)
            {
                return BadRequest(new { message = "Invalid year specified" });
            }

            _logger.LogInformation("Generating monthly report PDF for {Year}-{Month}", year, month);

            var pdfBytes = await _pdfService.GenerateMonthlyReportAsync(year, month);
            var monthName = new DateTime(year, month, 1).ToString("MMMM");
            var fileName = $"Monthly-Report-{monthName}-{year}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating monthly report for {Year}-{Month}", year, month);
            return StatusCode(500, new { message = "An error occurred while generating the monthly report" });
        }
    }

    /// <summary>
    /// Generate and download tenant list PDF
    /// </summary>
    /// <returns>PDF file download</returns>
    [HttpGet("tenant-list")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTenantList()
    {
        try
        {
            _logger.LogInformation("Generating tenant list PDF");

            var pdfBytes = await _pdfService.GenerateTenantListAsync();
            var fileName = $"Tenant-List-{DateTime.UtcNow:yyyyMMdd}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating tenant list");
            return StatusCode(500, new { message = "An error occurred while generating the tenant list" });
        }
    }

    /// <summary>
    /// Preview payment receipt in browser (inline display)
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <returns>PDF file for inline display</returns>
    [HttpGet("payment-receipt/{paymentId}/preview")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PreviewPaymentReceipt(int paymentId)
    {
        try
        {
            var pdfBytes = await _pdfService.GeneratePaymentReceiptAsync(paymentId);
            return File(pdfBytes, "application/pdf");
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error previewing payment receipt for payment {PaymentId}", paymentId);
            return StatusCode(500, new { message = "An error occurred while generating the preview" });
        }
    }

    /// <summary>
    /// Preview monthly report in browser (inline display)
    /// </summary>
    /// <param name="year">Year (e.g., 2024)</param>
    /// <param name="month">Month (1-12)</param>
    /// <returns>PDF file for inline display</returns>
    [HttpGet("monthly-report/{year}/{month}/preview")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PreviewMonthlyReport(int year, int month)
    {
        try
        {
            if (month < 1 || month > 12)
            {
                return BadRequest(new { message = "Month must be between 1 and 12" });
            }

            var pdfBytes = await _pdfService.GenerateMonthlyReportAsync(year, month);
            return File(pdfBytes, "application/pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error previewing monthly report for {Year}-{Month}", year, month);
            return StatusCode(500, new { message = "An error occurred while generating the preview" });
        }
    }

    /// <summary>
    /// Preview tenant list in browser (inline display)
    /// </summary>
    /// <returns>PDF file for inline display</returns>
    [HttpGet("tenant-list/preview")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PreviewTenantList()
    {
        try
        {
            var pdfBytes = await _pdfService.GenerateTenantListAsync();
            return File(pdfBytes, "application/pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error previewing tenant list");
            return StatusCode(500, new { message = "An error occurred while generating the preview" });
        }
    }
}
