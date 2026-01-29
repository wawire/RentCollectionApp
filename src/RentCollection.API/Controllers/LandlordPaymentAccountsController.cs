using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.Authorization;
using RentCollection.Application.DTOs.Payments;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.API.Controllers;

/// <summary>
/// Landlord payment account management endpoints
/// </summary>
[Authorize(Roles = "PlatformAdmin,Landlord")]
[Authorize(Policy = Policies.RequireVerifiedUser)]
[Authorize(Policy = Policies.RequireActiveOrganization)]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LandlordPaymentAccountsController : ControllerBase
{
    private readonly ILandlordPaymentAccountService _accountService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<LandlordPaymentAccountsController> _logger;

    public LandlordPaymentAccountsController(
        ILandlordPaymentAccountService accountService,
        ICurrentUserService currentUserService,
        ILogger<LandlordPaymentAccountsController> logger)
    {
        _accountService = accountService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Get all payment accounts for current landlord
    /// </summary>
    /// <returns>List of payment accounts</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyAccounts()
    {
        var userId = int.Parse(_currentUserService.UserId!);
        var result = await _accountService.GetLandlordAccountsAsync(userId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get payment accounts for a specific property
    /// </summary>
    /// <param name="propertyId">Property ID</param>
    /// <returns>List of payment accounts</returns>
    [HttpGet("property/{propertyId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPropertyAccounts(int propertyId)
    {
        var result = await _accountService.GetPropertyAccountsAsync(propertyId);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get payment account by ID
    /// </summary>
    /// <param name="id">Account ID</param>
    /// <returns>Payment account details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _accountService.GetAccountByIdAsync(id);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get default payment account
    /// </summary>
    /// <param name="propertyId">Optional property ID filter</param>
    /// <returns>Default payment account</returns>
    [HttpGet("default")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDefaultAccount([FromQuery] int? propertyId = null)
    {
        var userId = int.Parse(_currentUserService.UserId!);
        var result = await _accountService.GetDefaultAccountAsync(userId, propertyId);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Create a new payment account
    /// </summary>
    /// <param name="dto">Account creation data</param>
    /// <returns>Created payment account</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateLandlordPaymentAccountDto dto)
    {
        var userId = int.Parse(_currentUserService.UserId!);
        var result = await _accountService.CreateAccountAsync(userId, dto);

        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update an existing payment account
    /// </summary>
    /// <param name="id">Account ID</param>
    /// <param name="dto">Account update data</param>
    /// <returns>Updated payment account</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateLandlordPaymentAccountDto dto)
    {
        var result = await _accountService.UpdateAccountAsync(id, dto);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete a payment account
    /// </summary>
    /// <param name="id">Account ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _accountService.DeleteAccountAsync(id);

        if (!result.IsSuccess)
            return BadRequest(result);

        return NoContent();
    }

    /// <summary>
    /// Set an account as default
    /// </summary>
    /// <param name="id">Account ID to set as default</param>
    /// <returns>Success result</returns>
    [HttpPost("{id}/set-default")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetDefault(int id)
    {
        var result = await _accountService.SetDefaultAccountAsync(id);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}

