using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Common;
using RentCollection.Application.DTOs.Payments;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;
using System.Text.Json;

namespace RentCollection.Infrastructure.Services;

public class MPesaTransactionService : IMPesaTransactionService
{
    private readonly ApplicationDbContext _context;
    private readonly IMPesaService _mpesaService;
    private readonly ILogger<MPesaTransactionService> _logger;

    public MPesaTransactionService(
        ApplicationDbContext context,
        IMPesaService mpesaService,
        ILogger<MPesaTransactionService> logger)
    {
        _context = context;
        _mpesaService = mpesaService;
        _logger = logger;
    }

    public async Task<ServiceResult<bool>> HandleStkPushCallbackAsync(StkPushCallbackRequestDto callbackData)
    {
        try
        {
            if (callbackData.Body?.StkCallback == null)
            {
                return ServiceResult<bool>.Failure("Invalid callback data structure");
            }

            var callback = callbackData.Body.StkCallback;
            var checkoutRequestId = callback.CheckoutRequestID;

            if (string.IsNullOrEmpty(checkoutRequestId))
            {
                return ServiceResult<bool>.Failure("Missing CheckoutRequestID");
            }

            // Find the MPesa transaction record
            var transaction = await _context.MPesaTransactions
                .Include(t => t.Tenant)
                .FirstOrDefaultAsync(t => t.CheckoutRequestID == checkoutRequestId);

            if (transaction == null)
            {
                _logger.LogWarning("No M-Pesa transaction found for CheckoutRequestID: {CheckoutRequestID}", checkoutRequestId);
                return ServiceResult<bool>.Failure($"Transaction not found for CheckoutRequestID: {checkoutRequestId}");
            }

            // Update transaction with callback data
            transaction.ResultCode = callback.ResultCode;
            transaction.ResultDesc = callback.ResultDesc ?? "Unknown";
            transaction.CallbackJson = JsonSerializer.Serialize(callbackData);
            transaction.CallbackReceivedAt = DateTime.UtcNow;

            // Determine status from result code
            // 0 = Success, any other code = Failure
            if (callback.ResultCode == 0)
            {
                transaction.Status = MPesaTransactionStatus.Completed;

                // Extract callback metadata
                var callbackMetadata = callback.CallbackMetadata?.FirstOrDefault();
                if (callbackMetadata?.Item != null)
                {
                    foreach (var item in callbackMetadata.Item)
                    {
                        if (item.Name == "MpesaReceiptNumber")
                        {
                            transaction.MPesaReceiptNumber = item.Value?.ToString();
                        }
                        else if (item.Name == "TransactionDate" && item.Value != null)
                        {
                            // M-Pesa sends TransactionDate as a long (YYYYMMDDHHmmss)
                            if (long.TryParse(item.Value.ToString(), out var transDate))
                            {
                                var dateStr = transDate.ToString();
                                if (DateTime.TryParseExact(
                                    dateStr,
                                    "yyyyMMddHHmmss",
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    System.Globalization.DateTimeStyles.None,
                                    out var parsedDate))
                                {
                                    transaction.TransactionDate = parsedDate;
                                }
                            }
                        }
                    }
                }

                // Create payment record if successful
                if (transaction.TenantId.HasValue)
                {
                    await CreatePaymentFromMPesaTransactionAsync(transaction);
                }

                _logger.LogInformation(
                    "STK Push completed successfully: CheckoutRequestID={CheckoutRequestID}, Receipt={Receipt}",
                    checkoutRequestId, transaction.MPesaReceiptNumber);
            }
            else
            {
                // Failed, cancelled, or timeout
                transaction.Status = callback.ResultCode == 1032 ? MPesaTransactionStatus.Cancelled :
                                    callback.ResultCode == 2001 ? MPesaTransactionStatus.Timeout :
                                    MPesaTransactionStatus.Failed;

                _logger.LogWarning(
                    "STK Push failed/cancelled: CheckoutRequestID={CheckoutRequestID}, ResultCode={ResultCode}, ResultDesc={ResultDesc}",
                    checkoutRequestId, callback.ResultCode, callback.ResultDesc);
            }

            await _context.SaveChangesAsync();

            return ServiceResult<bool>.Success(true, "Callback processed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling STK Push callback");
            return ServiceResult<bool>.Failure($"Error processing callback: {ex.Message}");
        }
    }

    public async Task<ServiceResult<StkPushQueryResponseDto>> QueryAndUpdateStkPushStatusAsync(string checkoutRequestId)
    {
        try
        {
            // Query M-Pesa for the transaction status
            var queryResult = await _mpesaService.QueryStkPushStatusAsync(checkoutRequestId);

            if (!queryResult.IsSuccess)
            {
                return queryResult;
            }

            // Update local transaction record if found
            var transaction = await _context.MPesaTransactions
                .FirstOrDefaultAsync(t => t.CheckoutRequestID == checkoutRequestId);

            if (transaction != null)
            {
                var resultCode = -1;
                if (!string.IsNullOrWhiteSpace(queryResult.Data?.ResultCode) &&
                    int.TryParse(queryResult.Data.ResultCode, out var parsedCode))
                {
                    resultCode = parsedCode;
                }

                if (resultCode == 0)
                {
                    transaction.Status = MPesaTransactionStatus.Completed;
                    transaction.ResultCode = 0;
                    transaction.ResultDesc = queryResult.Data?.ResultDesc ?? "Completed";
                    transaction.UpdatedAt = DateTime.UtcNow;

                    if (!transaction.PaymentId.HasValue)
                    {
                        await CreatePaymentFromMPesaTransactionAsync(transaction);
                    }
                }
                else if (resultCode == 1032)
                {
                    transaction.Status = MPesaTransactionStatus.Cancelled;
                    transaction.ResultCode = resultCode;
                    transaction.ResultDesc = queryResult.Data?.ResultDesc ?? "Cancelled";
                    transaction.UpdatedAt = DateTime.UtcNow;
                }
                else if (resultCode == 1037)
                {
                    transaction.Status = MPesaTransactionStatus.Timeout;
                    transaction.ResultCode = resultCode;
                    transaction.ResultDesc = queryResult.Data?.ResultDesc ?? "Timeout";
                    transaction.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    transaction.Status = MPesaTransactionStatus.Failed;
                    transaction.ResultCode = resultCode;
                    transaction.ResultDesc = queryResult.Data?.ResultDesc ?? "Failed";
                    transaction.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }

            return queryResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying and updating STK Push status");
            return ServiceResult<StkPushQueryResponseDto>.Failure($"Error querying status: {ex.Message}");
        }
    }

    public async Task<List<string>> GetPendingStkPushCheckoutRequestIdsAsync(DateTime olderThanUtc, int batchSize)
    {
        return await _context.MPesaTransactions
            .Where(t => t.Status == MPesaTransactionStatus.Pending &&
                        (t.TransactionType == MPesaTransactionType.C2B || t.TransactionType == 0) &&
                        t.CreatedAt <= olderThanUtc &&
                        !string.IsNullOrEmpty(t.CheckoutRequestID))
            .OrderBy(t => t.CreatedAt)
            .Select(t => t.CheckoutRequestID)
            .Take(batchSize)
            .ToListAsync();
    }

    private async Task CreatePaymentFromMPesaTransactionAsync(MPesaTransaction mpesaTransaction)
    {
        try
        {
            if (mpesaTransaction.PaymentId.HasValue)
            {
                return;
            }

            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.TransactionReference == mpesaTransaction.MPesaReceiptNumber ||
                                          p.TransactionReference == mpesaTransaction.CheckoutRequestID);

            if (existingPayment != null)
            {
                mpesaTransaction.PaymentId = existingPayment.Id;
                await _context.SaveChangesAsync();
                return;
            }

            var tenant = await _context.Tenants
                .Include(t => t.Unit)
                    .ThenInclude(u => u.Property)
                        .ThenInclude(p => p.PaymentAccounts)
                .FirstOrDefaultAsync(t => t.Id == mpesaTransaction.TenantId);

            if (tenant == null)
            {
                _logger.LogError("Tenant not found for MPesaTransaction: {TransactionId}", mpesaTransaction.Id);
                return;
            }

            if (tenant.Unit == null || tenant.Unit.Property == null)
            {
                _logger.LogError(
                    "Tenant {TenantId} missing unit/property for MPesaTransaction: {TransactionId}",
                    tenant.Id,
                    mpesaTransaction.Id);
                return;
            }

            // Get the payment account
            var paymentAccount = tenant.Unit.Property.PaymentAccounts?
                .FirstOrDefault(pa => pa.AccountType == PaymentAccountType.MPesaPaybill && pa.IsActive);

            if (paymentAccount == null)
            {
                _logger.LogError("No active M-Pesa payment account found for tenant: {TenantId}", tenant.Id);
                return;
            }

            // Calculate due date for the current payment period
            var dueDate = Application.Helpers.PaymentDueDateHelper.CalculateNextMonthDueDate(tenant.RentDueDay);
            var (periodStart, periodEnd) = Application.Helpers.PaymentDueDateHelper.GetPaymentPeriod(dueDate);

            // Create payment record
            var payment = new Payment
            {
                TenantId = tenant.Id,
                UnitId = tenant.Unit.Id,
                LandlordAccountId = paymentAccount.Id,
                Amount = mpesaTransaction.Amount,
                PaymentDate = mpesaTransaction.TransactionDate ?? DateTime.UtcNow,
                DueDate = dueDate,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                PaymentMethod = PaymentMethod.MPesa,
                Status = PaymentStatus.Pending, // Still pending landlord confirmation
                UnallocatedAmount = mpesaTransaction.Amount,
                TransactionReference = mpesaTransaction.MPesaReceiptNumber ?? mpesaTransaction.CheckoutRequestID,
                PaybillAccountNumber = mpesaTransaction.AccountReference,
                MPesaPhoneNumber = mpesaTransaction.PhoneNumber,
                Notes = $"M-Pesa STK Push auto-recorded payment. Receipt: {mpesaTransaction.MPesaReceiptNumber}",
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Link payment to M-Pesa transaction after payment is persisted
            mpesaTransaction.PaymentId = payment.Id;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Payment record created from MPesa transaction: PaymentId={PaymentId}, MPesaReceipt={Receipt}, Amount={Amount}",
                payment.Id, mpesaTransaction.MPesaReceiptNumber, mpesaTransaction.Amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment from MPesa transaction: {TransactionId}", mpesaTransaction.Id);
        }
    }
}
