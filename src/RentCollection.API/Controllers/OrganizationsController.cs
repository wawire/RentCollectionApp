using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.Authorization;
using RentCollection.Application.DTOs.Auth;
using RentCollection.Application.DTOs.Organizations;
using RentCollection.Application.Services.Auth;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Enums;

namespace RentCollection.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "PlatformAdmin")]
[Authorize(Policy = Policies.RequireVerifiedUser)]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationService _organizationService;
    private readonly IAuthService _authService;

    public OrganizationsController(IOrganizationService organizationService, IAuthService authService)
    {
        _organizationService = organizationService;
        _authService = authService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrganizationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationDto dto)
    {
        var result = await _organizationService.CreateOrganizationAsync(dto);
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        if (result.Data == null)
        {
            return BadRequest(new { message = "Organization creation failed" });
        }

        return CreatedAtAction(nameof(GetOrganizationById), new { id = result.Data.Id }, result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrganizationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrganizations()
    {
        var result = await _organizationService.GetOrganizationsAsync();
        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrganizationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrganizationById(int id)
    {
        var result = await _organizationService.GetOrganizationByIdAsync(id);
        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(OrganizationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrganizationStatus(int id, [FromBody] OrganizationStatus status)
    {
        var result = await _organizationService.UpdateOrganizationStatusAsync(id, status);
        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    [HttpPost("{id}/users")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrganizationUser(int id, [FromBody] RegisterDto dto)
    {
        dto.OrganizationId = id;
        var response = await _authService.RegisterAsync(dto);
        return CreatedAtAction(nameof(GetOrganizationById), new { id }, response);
    }

    [HttpPost("{id}/properties/{propertyId}/assign-user")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignUserToProperty(int id, int propertyId, [FromBody] AssignUserToPropertyDto dto)
    {
        var result = await _organizationService.AssignUserToPropertyAsync(id, propertyId, dto);
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { message = result.Message ?? "User assigned to property" });
    }
}

