using RentCollection.Application.Common;
using RentCollection.Application.DTOs.Payments;

namespace RentCollection.Application.Services.Interfaces;

/// <summary>
/// Service for managing M-Pesa transaction records
/// </summary>
public interface IMPesaTransactionService
{
    /// <summary>
    /// Handle STK Push callback from Safaricom
    /// </summary>
    Task<ServiceResult<bool>> HandleStkPushCallbackAsync(StkPushCallbackRequestDto callbackData);

    /// <summary>
    /// Query STK Push transaction status and update database
    /// </summary>
    Task<ServiceResult<StkPushQueryResponseDto>> QueryAndUpdateStkPushStatusAsync(string checkoutRequestId);

    /// <summary>
    /// Get pending STK Push checkout request IDs older than a cutoff
    /// </summary>
    Task<List<string>> GetPendingStkPushCheckoutRequestIdsAsync(DateTime olderThanUtc, int batchSize);
}
