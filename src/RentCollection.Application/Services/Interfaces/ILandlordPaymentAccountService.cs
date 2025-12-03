using RentCollection.Application.Common;
using RentCollection.Application.DTOs.Payments;

namespace RentCollection.Application.Services.Interfaces;

/// <summary>
/// Service for managing landlord payment accounts
/// </summary>
public interface ILandlordPaymentAccountService
{
    /// <summary>
    /// Get all payment accounts for a landlord
    /// </summary>
    /// <param name="landlordId">Landlord ID</param>
    /// <returns>List of payment accounts</returns>
    Task<ServiceResult<List<LandlordPaymentAccountDto>>> GetLandlordAccountsAsync(int landlordId);

    /// <summary>
    /// Get payment accounts for a specific property
    /// </summary>
    /// <param name="propertyId">Property ID</param>
    /// <returns>List of payment accounts</returns>
    Task<ServiceResult<List<LandlordPaymentAccountDto>>> GetPropertyAccountsAsync(int propertyId);

    /// <summary>
    /// Get payment account by ID
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <returns>Payment account details</returns>
    Task<ServiceResult<LandlordPaymentAccountDto>> GetAccountByIdAsync(int accountId);

    /// <summary>
    /// Get default payment account for a landlord or property
    /// </summary>
    /// <param name="landlordId">Landlord ID</param>
    /// <param name="propertyId">Optional property ID</param>
    /// <returns>Default payment account</returns>
    Task<ServiceResult<LandlordPaymentAccountDto>> GetDefaultAccountAsync(int landlordId, int? propertyId = null);

    /// <summary>
    /// Create a new payment account
    /// </summary>
    /// <param name="landlordId">Landlord ID</param>
    /// <param name="dto">Account creation data</param>
    /// <returns>Created payment account</returns>
    Task<ServiceResult<LandlordPaymentAccountDto>> CreateAccountAsync(int landlordId, CreateLandlordPaymentAccountDto dto);

    /// <summary>
    /// Update an existing payment account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="dto">Account update data</param>
    /// <returns>Updated payment account</returns>
    Task<ServiceResult<LandlordPaymentAccountDto>> UpdateAccountAsync(int accountId, UpdateLandlordPaymentAccountDto dto);

    /// <summary>
    /// Delete a payment account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <returns>Result indicating success/failure</returns>
    Task<ServiceResult<bool>> DeleteAccountAsync(int accountId);

    /// <summary>
    /// Set an account as default for a landlord
    /// </summary>
    /// <param name="accountId">Account ID to set as default</param>
    /// <returns>Result indicating success/failure</returns>
    Task<ServiceResult<bool>> SetDefaultAccountAsync(int accountId);
}
