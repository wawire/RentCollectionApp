using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.Authorization;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Enums;

namespace RentCollection.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "PlatformAdmin,Landlord,Accountant")]
[Authorize(Policy = Policies.RequireVerifiedUser)]
[Authorize(Policy = Policies.RequireActiveOrganization)]
public class OwnerStatementsController : ControllerBase
{
    private readonly IPdfService _pdfService;
    private readonly IDocumentService _documentService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPropertyRepository _propertyRepository;

    public OwnerStatementsController(
        IPdfService pdfService,
        IDocumentService documentService,
        ICurrentUserService currentUserService,
        IPropertyRepository propertyRepository)
    {
        _pdfService = pdfService;
        _documentService = documentService;
        _currentUserService = currentUserService;
        _propertyRepository = propertyRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetStatement(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] int? propertyId = null,
        [FromQuery] int? landlordId = null)
    {
        if (year < 2000 || month < 1 || month > 12)
        {
            return BadRequest(new { message = "Invalid year or month" });
        }

        var resolvedScope = await ResolveLandlordIdAsync(landlordId, propertyId);
        if (!resolvedScope.IsAllowed || !resolvedScope.LandlordId.HasValue)
        {
            return BadRequest(new { message = "Landlord scope not found" });
        }

        if (!_currentUserService.UserIdInt.HasValue)
        {
            return BadRequest(new { message = "Authenticated user not found" });
        }

        var pdf = await _pdfService.GenerateOwnerStatementAsync(resolvedScope.LandlordId.Value, year, month, propertyId);
        var fileName = $"OwnerStatement_{resolvedScope.LandlordId}_{year}{month:00}.pdf";

        var description = propertyId.HasValue
            ? $"Owner statement for property {propertyId} - {year}-{month:00}"
            : $"Owner statement for landlord {resolvedScope.LandlordId} - {year}-{month:00}";

        var saveResult = await _documentService.SaveGeneratedDocumentAsync(
            DocumentType.OwnerStatement,
            pdf,
            fileName,
            "application/pdf",
            _currentUserService.UserIdInt.Value,
            landlordId: resolvedScope.LandlordId,
            propertyId: propertyId,
            description: description);

        if (!saveResult.IsSuccess)
        {
            var message = saveResult.ErrorMessage;
            if (message.Contains("permission", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            return BadRequest(new { message });
        }

        return File(pdf, "application/pdf", fileName);
    }

    private async Task<(bool IsAllowed, int? LandlordId)> ResolveLandlordIdAsync(int? landlordId, int? propertyId)
    {
        if (_currentUserService.IsPlatformAdmin)
        {
            return (true, landlordId);
        }

        if (_currentUserService.IsLandlord)
        {
            return (_currentUserService.UserIdInt.HasValue, _currentUserService.UserIdInt);
        }

        if (!_currentUserService.IsAccountant && !_currentUserService.IsManager && !_currentUserService.IsCaretaker)
        {
            return (false, null);
        }

        if (!propertyId.HasValue)
        {
            return (false, null);
        }

        var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
        if (!assignedPropertyIds.Contains(propertyId.Value))
        {
            return (false, null);
        }

        var property = await _propertyRepository.GetByIdAsync(propertyId.Value);
        if (property == null || !property.LandlordId.HasValue)
        {
            return (false, null);
        }

        return (true, property.LandlordId.Value);
    }
}

