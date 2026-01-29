using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.API.Controllers;

/// <summary>
/// Bulk import operations for CSV data
/// </summary>
[Authorize(Roles = "PlatformAdmin,Landlord")]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BulkImportController : ControllerBase
{
    private readonly IBulkImportService _bulkImportService;
    private readonly ILogger<BulkImportController> _logger;

    public BulkImportController(IBulkImportService bulkImportService, ILogger<BulkImportController> logger)
    {
        _bulkImportService = bulkImportService;
        _logger = logger;
    }

    /// <summary>
    /// Import tenants from CSV file
    /// </summary>
    /// <param name="file">CSV file with tenant data</param>
    /// <param name="propertyId">Property ID to assign tenants to</param>
    /// <returns>Import result with success/failure counts</returns>
    [HttpPost("tenants")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportTenants([FromForm] IFormFile file, [FromForm] int propertyId)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is required");
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only CSV files are supported");
        }

        var result = await _bulkImportService.ImportTenantsFromCsvAsync(file, propertyId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Import payments from CSV file
    /// </summary>
    /// <param name="file">CSV file with payment data</param>
    /// <returns>Import result with success/failure counts</returns>
    [HttpPost("payments")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportPayments([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is required");
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only CSV files are supported");
        }

        var result = await _bulkImportService.ImportPaymentsFromCsvAsync(file);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Validate tenant CSV without importing
    /// </summary>
    /// <param name="file">CSV file to validate</param>
    /// <param name="propertyId">Property ID</param>
    /// <returns>Validation result</returns>
    [HttpPost("tenants/validate")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidateTenantCsv([FromForm] IFormFile file, [FromForm] int propertyId)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is required");
        }

        var result = await _bulkImportService.ValidateTenantCsvAsync(file, propertyId);
        return Ok(result);
    }

    /// <summary>
    /// Validate payment CSV without importing
    /// </summary>
    /// <param name="file">CSV file to validate</param>
    /// <returns>Validation result</returns>
    [HttpPost("payments/validate")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidatePaymentCsv([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is required");
        }

        var result = await _bulkImportService.ValidatePaymentCsvAsync(file);
        return Ok(result);
    }
}

