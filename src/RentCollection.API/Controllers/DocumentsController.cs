using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.DTOs.Documents;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Enums;

namespace RentCollection.API.Controllers;

/// <summary>
/// Document management endpoints for lease agreements, ID copies, and other documents
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(IDocumentService documentService, ILogger<DocumentsController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a new document
    /// </summary>
    /// <param name="file">Document file (PDF, JPG, PNG, DOC, DOCX - Max 10MB)</param>
    /// <param name="documentType">Type of document</param>
    /// <param name="tenantId">Optional: Tenant ID if document belongs to a tenant</param>
    /// <param name="propertyId">Optional: Property ID if document belongs to a property</param>
    /// <param name="unitId">Optional: Unit ID if document belongs to a unit</param>
    /// <param name="description">Optional: Document description</param>
    /// <returns>Uploaded document information</returns>
    [HttpPost("upload")]
    [Authorize(Roles = "SystemAdmin,Landlord,Caretaker,Accountant,Tenant")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile file,
        [FromForm] DocumentType documentType,
        [FromForm] int? tenantId = null,
        [FromForm] int? propertyId = null,
        [FromForm] int? unitId = null,
        [FromForm] string? description = null)
    {
        if (file == null)
        {
            return BadRequest("File is required");
        }

        var uploadDto = new UploadDocumentDto
        {
            File = file,
            DocumentType = documentType,
            TenantId = tenantId,
            PropertyId = propertyId,
            UnitId = unitId,
            Description = description
        };

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int uploadedByUserId))
        {
            return Unauthorized("Invalid user ID");
        }

        var result = await _documentService.UploadDocumentAsync(uploadDto, uploadedByUserId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get all documents (filtered by RBAC)
    /// </summary>
    /// <returns>List of documents</returns>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin,Landlord,Caretaker,Accountant,Tenant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _documentService.GetAllDocumentsAsync();

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get document by ID
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <returns>Document information</returns>
    [HttpGet("{id}")]
    [Authorize(Roles = "SystemAdmin,Landlord,Caretaker,Accountant,Tenant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _documentService.GetDocumentByIdAsync(id);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get all documents for a specific tenant
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>List of tenant documents</returns>
    [HttpGet("tenant/{tenantId}")]
    [Authorize(Roles = "SystemAdmin,Landlord,Caretaker,Accountant,Tenant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByTenantId(int tenantId)
    {
        var result = await _documentService.GetDocumentsByTenantIdAsync(tenantId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get all documents for a specific property
    /// </summary>
    /// <param name="propertyId">Property ID</param>
    /// <returns>List of property documents</returns>
    [HttpGet("property/{propertyId}")]
    [Authorize(Roles = "SystemAdmin,Landlord,Caretaker,Accountant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByPropertyId(int propertyId)
    {
        var result = await _documentService.GetDocumentsByPropertyIdAsync(propertyId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get all documents for a specific unit
    /// </summary>
    /// <param name="unitId">Unit ID</param>
    /// <returns>List of unit documents</returns>
    [HttpGet("unit/{unitId}")]
    [Authorize(Roles = "SystemAdmin,Landlord,Caretaker,Accountant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByUnitId(int unitId)
    {
        var result = await _documentService.GetDocumentsByUnitIdAsync(unitId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get all documents of a specific type
    /// </summary>
    /// <param name="documentType">Document type</param>
    /// <returns>List of documents of the specified type</returns>
    [HttpGet("type/{documentType}")]
    [Authorize(Roles = "SystemAdmin,Landlord,Caretaker,Accountant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByType(DocumentType documentType)
    {
        var result = await _documentService.GetDocumentsByTypeAsync(documentType);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get all unverified documents
    /// </summary>
    /// <returns>List of unverified documents</returns>
    [HttpGet("unverified")]
    [Authorize(Roles = "SystemAdmin,Landlord,Accountant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUnverified()
    {
        var result = await _documentService.GetUnverifiedDocumentsAsync();

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get my documents (for current tenant user)
    /// </summary>
    /// <returns>List of current tenant's documents</returns>
    [HttpGet("me")]
    [Authorize(Roles = "Tenant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMyDocuments()
    {
        var result = await _documentService.GetMyDocumentsAsync();

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Verify/approve a document (landlord/admin only)
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <param name="verifyDto">Verification data</param>
    /// <returns>Updated document</returns>
    [HttpPatch("{id}/verify")]
    [Authorize(Roles = "SystemAdmin,Landlord,Accountant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Verify(int id, [FromBody] VerifyDocumentDto verifyDto)
    {
        var result = await _documentService.VerifyDocumentAsync(id, verifyDto.IsVerified);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete a document
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <returns>Success message</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SystemAdmin,Landlord,Accountant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _documentService.DeleteDocumentAsync(id);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}
