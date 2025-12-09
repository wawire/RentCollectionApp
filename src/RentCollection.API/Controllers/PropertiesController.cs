using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.DTOs.Properties;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.API.Controllers;

/// <summary>
/// Properties management endpoints
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PropertiesController : ControllerBase
{
    private readonly IPropertyService _propertyService;
    private readonly IPdfService _pdfService;
    private readonly ILogger<PropertiesController> _logger;

    public PropertiesController(IPropertyService propertyService, IPdfService pdfService, ILogger<PropertiesController> logger)
    {
        _propertyService = propertyService;
        _pdfService = pdfService;
        _logger = logger;
    }

    /// <summary>
    /// Get all properties
    /// </summary>
    /// <returns>List of all properties</returns>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin,Landlord,Caretaker,Accountant,Tenant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _propertyService.GetAllPropertiesAsync();

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get paginated properties
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <returns>Paginated list of properties</returns>
    [HttpGet("paginated")]
    [Authorize(Roles = "SystemAdmin,Landlord,Caretaker,Accountant,Tenant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _propertyService.GetPropertiesPaginatedAsync(pageNumber, pageSize);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get property by ID
    /// </summary>
    /// <param name="id">Property ID</param>
    /// <returns>Property details</returns>
    [HttpGet("{id}")]
    [Authorize(Roles = "SystemAdmin,Landlord,Caretaker,Accountant,Tenant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _propertyService.GetPropertyByIdAsync(id);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Create a new property
    /// </summary>
    /// <param name="createDto">Property creation data</param>
    /// <returns>Created property</returns>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin,Landlord")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePropertyDto createDto)
    {
        var result = await _propertyService.CreatePropertyAsync(createDto);

        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update an existing property
    /// </summary>
    /// <param name="id">Property ID</param>
    /// <param name="updateDto">Property update data</param>
    /// <returns>Updated property</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "SystemAdmin,Landlord")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePropertyDto updateDto)
    {
        var result = await _propertyService.UpdatePropertyAsync(id, updateDto);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete a property
    /// </summary>
    /// <param name="id">Property ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SystemAdmin,Landlord")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _propertyService.DeletePropertyAsync(id);

        if (!result.IsSuccess)
            return BadRequest(result);

        return NoContent();
    }

    /// <summary>
    /// Upload property image
    /// </summary>
    /// <param name="id">Property ID</param>
    /// <param name="file">Image file (JPG, PNG, WEBP, max 5MB)</param>
    /// <returns>Updated property with new image URL</returns>
    [HttpPost("{id}/image")]
    [Authorize(Roles = "SystemAdmin,Landlord")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UploadImage(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { isSuccess = false, message = "No file provided" });
        }

        var result = await _propertyService.UploadPropertyImageAsync(id, file);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete property image
    /// </summary>
    /// <param name="id">Property ID</param>
    /// <returns>Success message</returns>
    [HttpDelete("{id}/image")]
    [Authorize(Roles = "SystemAdmin,Landlord")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteImage(int id)
    {
        var result = await _propertyService.DeletePropertyImageAsync(id);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Export property payment history as PDF
    /// </summary>
    /// <param name="id">Property ID</param>
    /// <param name="startDate">Optional start date for filtering payments</param>
    /// <param name="endDate">Optional end date for filtering payments</param>
    [HttpGet("{id}/payment-history/export")]
    [Authorize(Roles = "SystemAdmin,Landlord")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportPropertyPaymentHistory(int id, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            // Generate PDF
            var pdfBytes = await _pdfService.GeneratePropertyPaymentHistoryAsync(id, startDate, endDate);

            // Return PDF file
            var fileName = $"Property_{id}_Payment_History_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Property {PropertyId} not found", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting property payment history for property {PropertyId}", id);
            return BadRequest(new { message = "Failed to export property payment history" });
        }
    }
}
