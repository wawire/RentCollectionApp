using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.DTOs.MoveOutInspections;
using RentCollection.Application.DTOs.Payments;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Enums;
using System.Security.Claims;

namespace RentCollection.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MoveOutInspectionsController : ControllerBase
{
    private readonly IMoveOutInspectionService _inspectionService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IPdfService _pdfService;
    private readonly IMPesaService _mpesaService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<MoveOutInspectionsController> _logger;

    public MoveOutInspectionsController(
        IMoveOutInspectionService inspectionService,
        IFileStorageService fileStorageService,
        IPdfService pdfService,
        IMPesaService mpesaService,
        ICurrentUserService currentUserService,
        ILogger<MoveOutInspectionsController> logger)
    {
        _inspectionService = inspectionService;
        _fileStorageService = fileStorageService;
        _pdfService = pdfService;
        _mpesaService = mpesaService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Get all pending inspections (Landlord/PlatformAdmin only)
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Landlord,PlatformAdmin,Manager,Caretaker")]
    public async Task<IActionResult> GetPendingInspections()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _inspectionService.GetPendingInspectionsAsync(userId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get inspections by status (Landlord/PlatformAdmin only)
    /// </summary>
    [HttpGet("status/{status}")]
    [Authorize(Roles = "Landlord,PlatformAdmin,Manager,Caretaker")]
    public async Task<IActionResult> GetInspectionsByStatus(MoveOutInspectionStatus status)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _inspectionService.GetInspectionsByStatusAsync(status, userId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get inspection by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetInspectionById(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _inspectionService.GetInspectionByIdAsync(id, userId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get all inspections for a tenant
    /// </summary>
    [HttpGet("tenant/{tenantId}")]
    public async Task<IActionResult> GetTenantInspections(int tenantId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        var userTenantId = User.FindFirst("TenantId")?.Value;

        // Tenants can only view their own inspections
        if (userRole == "Tenant" && userTenantId != tenantId.ToString())
            return Forbid();

        var result = await _inspectionService.GetTenantInspectionsAsync(tenantId, userId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get all inspections for the current tenant
    /// </summary>
    [HttpGet("me")]
    [Authorize(Roles = "Tenant")]
    public async Task<IActionResult> GetMyInspections()
    {
        var tenantId = _currentUserService.TenantId;
        if (!tenantId.HasValue)
            return BadRequest(new { error = "User is not a tenant" });

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _inspectionService.GetTenantInspectionsAsync(tenantId.Value, userId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get all inspections for a property (Landlord/PlatformAdmin only)
    /// </summary>
    [HttpGet("property/{propertyId}")]
    [Authorize(Roles = "Landlord,PlatformAdmin,Manager,Caretaker")]
    public async Task<IActionResult> GetPropertyInspections(int propertyId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _inspectionService.GetPropertyInspectionsAsync(propertyId, userId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Schedule a new move-out inspection (Landlord/PlatformAdmin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Landlord,PlatformAdmin,Manager,Caretaker")]
    public async Task<IActionResult> ScheduleInspection([FromBody] CreateMoveOutInspectionDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _inspectionService.ScheduleInspectionAsync(dto, userId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return CreatedAtAction(nameof(GetInspectionById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Record inspection results and calculate deductions (Landlord/PlatformAdmin only)
    /// </summary>
    [HttpPost("{id}/record")]
    [Authorize(Roles = "Landlord,PlatformAdmin,Manager,Caretaker")]
    public async Task<IActionResult> RecordInspection(int id, [FromBody] RecordInspectionDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _inspectionService.RecordInspectionAsync(id, dto, userId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Settle the inspection and finalize deductions (Landlord/PlatformAdmin only)
    /// </summary>
    [HttpPost("{id}/settle")]
    [Authorize(Roles = "Landlord,PlatformAdmin")]
    public async Task<IActionResult> SettleInspection(int id, [FromBody] SettleInspectionDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _inspectionService.SettleInspectionAsync(id, dto, userId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Process refund to tenant (Landlord/PlatformAdmin only)
    /// </summary>
    [HttpPost("{id}/refund")]
    [Authorize(Roles = "Landlord,PlatformAdmin")]
    public async Task<IActionResult> ProcessRefund(int id, [FromBody] ProcessRefundDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _inspectionService.ProcessRefundAsync(id, dto, userId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Upload photo file for inspection (Landlord/PlatformAdmin only)
    /// </summary>
    [HttpPost("{id}/upload-photo")]
    [Authorize(Roles = "Landlord,PlatformAdmin,Manager,Caretaker")]
    [RequestSizeLimit(10_485_760)] // 10MB limit
    public async Task<IActionResult> UploadPhotoFile(
        int id,
        [FromForm] IFormFile file,
        [FromForm] string? caption,
        [FromForm] PhotoType photoType,
        [FromForm] int? inspectionItemId)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file provided" });

        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Validate file
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var (isValid, errorMessage) = await _fileStorageService.ValidateFileAsync(
                file, allowedExtensions, 10_485_760);

            if (!isValid)
                return BadRequest(new { error = errorMessage });

            // Upload file
            var photoUrl = await _fileStorageService.UploadFileAsync(file, $"inspections/{id}");

            // Save photo metadata to database
            var result = await _inspectionService.UploadPhotoAsync(
                id,
                photoUrl,
                caption,
                photoType,
                inspectionItemId,
                userId);

            if (!result.IsSuccess)
            {
                // Clean up uploaded file if database save failed
                await _fileStorageService.DeleteFileAsync(photoUrl);
                return BadRequest(new { error = result.ErrorMessage });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading inspection photo for inspection {InspectionId}", id);
            return StatusCode(500, new { error = "Failed to upload photo" });
        }
    }

    /// <summary>
    /// Upload photo for inspection (Landlord/PlatformAdmin only)
    /// </summary>
    [HttpPost("{id}/photos")]
    [Authorize(Roles = "Landlord,PlatformAdmin,Manager,Caretaker")]
    public async Task<IActionResult> UploadPhoto(
        int id,
        [FromBody] UploadPhotoRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _inspectionService.UploadPhotoAsync(
            id,
            request.PhotoUrl,
            request.Caption,
            request.PhotoType,
            request.InspectionItemId,
            userId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete inspection photo (Landlord/PlatformAdmin only)
    /// </summary>
    [HttpDelete("photos/{photoId}")]
    [Authorize(Roles = "Landlord,PlatformAdmin,Manager,Caretaker")]
    public async Task<IActionResult> DeletePhoto(int photoId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _inspectionService.DeletePhotoAsync(photoId, userId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return NoContent();
    }

    /// <summary>
    /// Generate and download PDF report for inspection
    /// </summary>
    [HttpGet("{id}/report")]
    public async Task<IActionResult> GenerateReport(int id)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Check if inspection exists and user has access
            var inspection = await _inspectionService.GetInspectionByIdAsync(id, userId);
            if (!inspection.IsSuccess)
                return BadRequest(new { error = inspection.ErrorMessage });

            var pdfBytes = await _pdfService.GenerateMoveOutInspectionReportAsync(id);

            return File(pdfBytes, "application/pdf", $"MoveOutInspection_{id}_{DateTime.Now:yyyyMMdd}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF report for inspection {InspectionId}", id);
            return StatusCode(500, new { error = "Failed to generate PDF report" });
        }
    }

    /// <summary>
    /// Process refund via M-Pesa B2C (Landlord/PlatformAdmin only)
    /// </summary>
    [HttpPost("{id}/refund-mpesa")]
    [Authorize(Roles = "Landlord,PlatformAdmin")]
    public async Task<IActionResult> ProcessMPesaRefund(int id, [FromBody] InitiateB2CDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Check if inspection exists and user has access
            var inspection = await _inspectionService.GetInspectionByIdAsync(id, userId);
            if (!inspection.IsSuccess)
                return BadRequest(new { error = inspection.ErrorMessage });

            // Initiate B2C disbursement
            var result = await _mpesaService.InitiateB2CAsync(id, dto);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.ErrorMessage });

            return Ok(new
            {
                message = "M-Pesa refund initiated successfully",
                conversationId = result.Data!.ConversationID,
                responseDescription = result.Data.ResponseDescription
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing M-Pesa refund for inspection {InspectionId}", id);
            return StatusCode(500, new { error = "Failed to process M-Pesa refund" });
        }
    }
}

/// <summary>
/// Request model for uploading photos
/// </summary>
public class UploadPhotoRequest
{
    public string PhotoUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public PhotoType PhotoType { get; set; }
    public int? InspectionItemId { get; set; }
}

