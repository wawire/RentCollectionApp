using RentCollection.Application.Common;
using RentCollection.Application.DTOs.Payments;

namespace RentCollection.Application.Services.Interfaces;

/// <summary>
/// Service for M-Pesa integration (STK Push, C2B, webhooks)
/// </summary>
public interface IMPesaService
{
    /// <summary>
    /// Initiate STK Push to tenant's phone for rent payment
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="dto">STK Push details</param>
    /// <returns>Result with STK Push response</returns>
    Task<ServiceResult<StkPushCallbackDto>> InitiateStkPushAsync(int tenantId, InitiateStkPushDto dto);

    /// <summary>
    /// Query STK Push payment status
    /// </summary>
    /// <param name="checkoutRequestId">Checkout Request ID from STK Push</param>
    /// <returns>Result with payment status</returns>
    Task<ServiceResult<StkPushQueryResponseDto>> QueryStkPushStatusAsync(string checkoutRequestId);

    /// <summary>
    /// Handle M-Pesa C2B (Customer to Business) callback for Paybill payments
    /// </summary>
    /// <param name="callbackData">Callback data from M-Pesa</param>
    /// <param name="correlationId">Optional correlation ID for logging</param>
    /// <returns>Result indicating success/failure</returns>
    Task<ServiceResult<bool>> HandleC2BCallbackAsync(MPesaC2BCallbackDto callbackData, string? correlationId = null);

    /// <summary>
    /// Get M-Pesa access token
    /// </summary>
    /// <param name="landlordAccountId">Landlord payment account ID</param>
    /// <returns>Access token</returns>
    Task<ServiceResult<string>> GetAccessTokenAsync(int landlordAccountId);

    /// <summary>
    /// Initiate B2C (Business to Customer) disbursement for refunds
    /// </summary>
    /// <param name="moveOutInspectionId">Move-out inspection ID</param>
    /// <param name="dto">B2C disbursement details</param>
    /// <returns>Result with B2C response</returns>
    Task<ServiceResult<B2CResponseDto>> InitiateB2CAsync(int moveOutInspectionId, InitiateB2CDto dto);

    /// <summary>
    /// Handle M-Pesa B2C result callback
    /// </summary>
    /// <param name="callbackData">Callback data from M-Pesa</param>
    /// <returns>Result indicating success/failure</returns>
    Task<ServiceResult<bool>> HandleB2CCallbackAsync(B2CCallbackDto callbackData);
}
