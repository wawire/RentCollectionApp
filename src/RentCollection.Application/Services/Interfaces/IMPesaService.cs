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
    Task<ServiceResult<StkPushCallbackDto>> QueryStkPushStatusAsync(string checkoutRequestId);

    /// <summary>
    /// Handle M-Pesa C2B (Customer to Business) callback for Paybill payments
    /// </summary>
    /// <param name="callbackData">Callback data from M-Pesa</param>
    /// <returns>Result indicating success/failure</returns>
    Task<ServiceResult<bool>> HandleC2BCallbackAsync(object callbackData);

    /// <summary>
    /// Get M-Pesa access token
    /// </summary>
    /// <param name="landlordAccountId">Landlord payment account ID</param>
    /// <returns>Access token</returns>
    Task<ServiceResult<string>> GetAccessTokenAsync(int landlordAccountId);
}
